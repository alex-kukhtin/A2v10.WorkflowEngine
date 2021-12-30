// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
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
}

public class SqlServerInstanceStorage : IInstanceStorage
{
	private readonly IDbContext _dbContext;
	private readonly IDbIdentity _dbIdentity;
	private readonly IWorkflowStorage _workflowStorage;
	private readonly ISerializer _serializer;

	public SqlServerInstanceStorage(IDbContext dbContext, IDbIdentity dbIdentity, IWorkflowStorage workflowStorage, ISerializer serializer)
	{
		_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		_workflowStorage = workflowStorage ?? throw new ArgumentNullException(nameof(workflowStorage));
		_serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
		_dbIdentity = dbIdentity;
	}

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
		_dbIdentity.SetIdentityParams(prms);
		var dbi = await _dbContext.LoadAsync<DatabaseInstance>(null, $"{SqlDefinitions.SqlSchema}.[Instance.{suffix}]", prms);
		if (dbi == null)
			throw new SqlServerStorageException($"Instance '{instanceId}' not found");

		var identity = new WorkflowIdentity (
			dbi.WorkflowId ?? throw new InvalidProgramException("WorkflowId is null"),
			dbi.Version
		);

		var wf = await _workflowStorage.LoadAsync(identity);

		return new Instance(wf, instanceId)
		{
			Parent = dbi.Parent,
			State = _serializer.Deserialize(dbi.State),
			ExecutionStatus = dbi.ExecutionStatus,
			Lock = dbi.Lock
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
			{ "ExecutionStatus", instance.ExecutionStatus.ToString() }
		};
		_dbIdentity.SetIdentityParams(ieo);
		await _dbContext.ExecuteExpandoAsync(null, $"{SqlDefinitions.SqlSchema}.[Instance.Create]", ieo);
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
		if (instanceData?.Deferred != null)
		{
			batches = new List<BatchProcedure>();
			foreach (var defer in instanceData.Deferred.Where(d => d.Type == DeferredElementType.Sql))
			{
				var epxParam = defer.Parameters.Clone();
				_dbIdentity.SetIdentityParams(epxParam);
				epxParam.Add("InstanceId", instance.Id);
				epxParam.Add("Activity", defer.Refer);
				batches.Add(new BatchProcedure(defer.Name, epxParam));
			}
		}

		_ = await _dbContext.SaveModelBatchAsync(null, $"{SqlDefinitions.SqlSchema}.[Instance.Update]", root, null, batches);
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
		return _dbContext.ExecuteAsync<SqlTrackRecord>(null, $"{SqlDefinitions.SqlSchema}.[Instance.Exception]", tr);
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
		var dm = await _dbContext.LoadModelAsync(null, $"{SqlDefinitions.SqlSchema}.[Instance.Pending.Load]", null);
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
		return _dbContext.ExecuteExpandoAsync(null, $"{SqlDefinitions.SqlSchema}.[AutoStart.Complete]", prms);
    }

	public async Task<IInstance?> LoadBookmark(String bookmark)
    {
		var prms = new ExpandoObject()
		{
			{ "Bookmark", bookmark }
		};
		_dbIdentity.SetIdentityParams(prms);
		var dbi = await _dbContext.LoadAsync<DatabaseInstance>(null, $"{SqlDefinitions.SqlSchema}.[Instance.LoadBookmark]", prms);
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
			ExecutionStatus = dbi.ExecutionStatus ,
			Lock = dbi.Lock
		};
	}

	#endregion

}

