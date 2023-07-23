// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.SqlServer;

public record DatabaseInstance
{
    public Guid? Id { get; set; }
    public Guid? Parent { get; set; }
    public Int32 Version { get; set; }
    public String? WorkflowId { get; set; }
    public String State { get; set; } = String.Empty;
    public WorkflowExecutionStatus ExecutionStatus { get; set; }

    public Guid? Lock { get; set; }

    public String? CorrelationId { get; set; }
}

public record DbDate
{
    public DateTime CurrentDate { get; set; }
}

public class SqlServerInstanceStorage : IInstanceStorage
{
    private readonly IDbContext _dbContext;
    private readonly IWorkflowStorage _workflowStorage;
    private readonly ISerializer _serializer;
    private readonly IDataSourceProvider _dataSourceProvider;

    public SqlServerInstanceStorage(IDbContext dbContext, IWorkflowStorage workflowStorage, ISerializer serializer,
        IDataSourceProvider dataSouceProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _workflowStorage = workflowStorage ?? throw new ArgumentNullException(nameof(workflowStorage));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _dataSourceProvider = dataSouceProvider ?? throw new ArgumentNullException(nameof(dataSouceProvider));
    }

    private String? DataSource => _dataSourceProvider.DataSource;

    #region IInstanceStorage
    public Task<IInstance> Load(Guid instanceId)
    {
        return LoadImpl(instanceId, "Load");
    }

    public Task<IInstance> LoadRaw(Guid instanceId)
    {
        return LoadImpl(instanceId, "LoadRaw");
    }

    private async Task<IInstance> LoadImpl(Guid instanceId, String suffix)
    {
        var prms = new ExpandoObject()
        {
            { "Id", instanceId }
        };
        _dataSourceProvider.SetIdentityParams(prms);
        var dbi = await _dbContext.LoadAsync<DatabaseInstance>(DataSource, $"{SqlDefinitions.SqlSchema}.[Instance.{suffix}]", prms) 
            ?? throw new SqlServerStorageException($"Instance '{instanceId}' not found");
        var identity = new WorkflowIdentity(
            dbi.WorkflowId ?? throw new InvalidProgramException("WorkflowId is null"),
            dbi.Version
        );

        var wf = await _workflowStorage.LoadAsync(identity);

        return new Instance(wf, instanceId)
        {
            Parent = dbi.Parent,
            State = _serializer.Deserialize(dbi.State),
            ExecutionStatus = dbi.ExecutionStatus,
            Lock = dbi.Lock,
            CorrelationId = dbi.CorrelationId
        };
    }

    public async Task Create(IInstance instance)
    {
        var ieo = new ExpandoObject()
        {
            { "Id", instance.Id },
            { "Parent", instance.Parent},
            { "Version", instance.Workflow.Identity.Version},
            { "WorkflowId", instance.Workflow.Identity.Id },
            { "ExecutionStatus", instance.ExecutionStatus.ToString() },
            { "CorrelationId", instance.CorrelationId },
        };
        _dataSourceProvider.SetIdentityParams(ieo);
        await _dbContext.ExecuteExpandoAsync(DataSource, $"{SqlDefinitions.SqlSchema}.[Instance.Create]", ieo);
    }

    public async Task Save(IInstance instance)
    {
        var instanceData = instance.InstanceData;

        var ieo = new ExpandoObject()
        {
            { "Id", instance.Id },
            { "WorkflowId", instance.Workflow.Identity.Id},
            { "ExecutionStatus", instance.ExecutionStatus.ToString() },
            { "Lock", instance.Lock },
            { "CorrelationId", instance.CorrelationId },
            { "State", _serializer.Serialize(instance.State) },
            { "Variables", instanceData?.ExternalVariables },
            { "Bookmarks", instanceData?.ExternalBookmarks},
            { "Events", instanceData?.ExternalEvents},
            { "TrackRecords", instanceData?.TrackRecords }
        };

        var root = new ExpandoObject()
        {
            {"Instance", ieo }
        };

        List<BatchProcedure>? batches = null;
        if (instanceData?.HasBatches == true)
        {
            batches = new List<BatchProcedure>();
            if (instanceData?.Deferred != null)
            {
                foreach (var defer in instanceData.Deferred.Where(d => d.Type == DeferredElementType.Sql))
                {
                    var epxParam = defer.Parameters.Clone();
                    _dataSourceProvider.SetIdentityParams(epxParam);
                    epxParam.Add("InstanceId", instance.Id);
                    epxParam.Add("Activity", defer.Refer);
                    batches.Add(new BatchProcedure(defer.Name, epxParam));
                }
            }
            var inboxes = instanceData?.Inboxes;
            if (inboxes != null)
            {
                foreach (var inboxCreate in inboxes.InboxCreate)
                {
                    var eo = inboxCreate.Clone();
                    _dataSourceProvider.SetIdentityParams(eo);
                    eo.Set("InstanceId", instance.Id);
                    batches.Add(new BatchProcedure($"{SqlDefinitions.SqlSchema}.[Instance.Inbox.Create]", eo));
                }
                foreach (var inboxDelete in inboxes.InboxRemove)
                {
                    var eo = new ExpandoObject();
                    _dataSourceProvider.SetIdentityParams(eo);
                    eo.Set("Id", inboxDelete);
                    eo.Set("InstanceId", instance.Id);
                    batches.Add(new BatchProcedure($"{SqlDefinitions.SqlSchema}.[Instance.Inbox.Remove]", eo));
                }
            }
        }

        _ = await _dbContext.SaveModelBatchAsync(DataSource, $"{SqlDefinitions.SqlSchema}.[Instance.Update]", root, null, batches);
    }


    public Task WriteException(Guid id, Exception ex)
    {
        var tr = new SqlTrackRecord()
        {
            Action = ActivityTrackAction.Exception,
            Kind = TrackRecordKind.Activity,
            InstanceId = id,
            Message = ex.ToString()
        };
        return _dbContext.ExecuteAsync<SqlTrackRecord>(DataSource, $"{SqlDefinitions.SqlSchema}.[Instance.Exception]", tr);
    }


    static List<IPendingInstance> PendingFromModel(ExpandoObject root)
    {
        var result = new List<IPendingInstance>();
        var pendList = root.Get<List<ExpandoObject>>("Pending");
        if (pendList == null)
            return result;
        // group by InstanceId - may be multiply
        var pendInstances = new Dictionary<Guid, PendingInstance>();
        foreach (var pe in pendList)
        {
            if (pe is not ExpandoObject pee)
                continue;
            var instId = pee.GetNotNull<Guid>("InstanceId");
            var eventKey = pee.Get<String>("EventKey") ?? throw new WorkflowException("Invalid event key");
            if (!pendInstances.TryGetValue(instId, out PendingInstance? pendingInstance))
            {
                pendingInstance = new PendingInstance() { InstanceId = instId };
                pendInstances.Add(instId, pendingInstance);
            }
            pendingInstance.AddEventKey(eventKey);
        }
        foreach (var pi in pendInstances.Values)
            result.Add(pi);
        return result;
    }

    static List<IAutoStartInstance> AutoStartFromModel(ExpandoObject root)
    {
        var result = new List<IAutoStartInstance>();

        var autoStartList = root.Get<List<ExpandoObject>>("AutoStart");
        if (autoStartList == null)
            return result;
        foreach (var asi in autoStartList)
        {
            if (asi is not ExpandoObject asie)
                continue;
            result.Add(new AutoStartInstance()
            {
                Id = asie.GetNotNull<Int64>("Id"),
                WorkflowId = asie.GetNotNull<String>("WorkflowId"),
                Version = asie.Get<Int32>("Version"),
                Params = asie.Get<ExpandoObject>("Params")
            });
        }
        return result;
    }


    public async Task<PendingElement?> GetPendingAsync()
    {
        var dm = await _dbContext.LoadModelAsync(DataSource, $"{SqlDefinitions.SqlSchema}.[Instance.Pending.Load]", null);
        if (dm == null || dm.Root == null)
            return null;

        return new PendingElement(Pending: PendingFromModel(dm.Root), AutoStart: AutoStartFromModel(dm.Root));
    }

    public Task AutoStartComplete(Int64 Id, Guid instanceId)
    {
        var prms = new ExpandoObject()
        {
            { "Id", Id },
            { "InstanceId", instanceId }
        };
        return _dbContext.ExecuteExpandoAsync(DataSource, $"{SqlDefinitions.SqlSchema}.[AutoStart.Complete]", prms);
    }

    public async Task<IInstance?> LoadBookmark(String bookmark)
    {
        var prms = new ExpandoObject()
        {
            { "Bookmark", bookmark }
        };
        _dataSourceProvider.SetIdentityParams(prms);
        var dbi = await _dbContext.LoadAsync<DatabaseInstance>(DataSource, $"{SqlDefinitions.SqlSchema}.[Instance.LoadBookmark]", prms);
        if (dbi == null)
            return null;

        var identity = new WorkflowIdentity(
            dbi.WorkflowId ?? throw new InvalidProgramException("WorkflowId is null"),
            dbi.Version
        );

        var wf = await _workflowStorage.LoadAsync(identity);
        return new Instance(wf, dbi.Id ?? throw new InvalidProgramException("InstanceId is null"))
        {
            Parent = dbi.Parent,
            State = _serializer.Deserialize(dbi.State),
            ExecutionStatus = dbi.ExecutionStatus,
            Lock = dbi.Lock,
            CorrelationId = dbi.CorrelationId
        };
    }

    public async ValueTask<DateTime> GetNowTime()
	{
        var dbDate = await _dbContext.LoadAsync<DbDate>(DataSource, "a2wf.[CurrentDate.Get]");
        return dbDate?.CurrentDate ?? throw new SqlServerStorageException("CurrentDate is null");
	}
    #endregion

}

