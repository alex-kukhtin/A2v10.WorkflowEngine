// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.SqlServer
{
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

		void SetIdentityParams(ExpandoObject eo)
		{
			if (_dbIdentity.TenantId.HasValue)
				eo.Set("TenantId", _dbIdentity.TenantId);
			if (_dbIdentity.UserId.HasValue)
				eo.Set("UserId", _dbIdentity.UserId);
		}

		#region IInstanceStorage
		public async Task<IInstance> Load(Guid instanceId)
		{
			var prms = new ExpandoObject()
			{
				{ "Id", instanceId }
			};
			SetIdentityParams(prms);
			var eo = await _dbContext.ReadExpandoAsync(null, $"{SqlDefinitions.SqlSchema}.[Instance.Load]", prms);
			if (eo == null)
				throw new SqlServerStorageException($"Instance '{instanceId}' not found");

			var identity = new WorkflowIdentity()
			{
				Id = eo.Get<String>("WorkflowId"),
				Version = eo.Get<Int32>("Version")
			};

			return new Instance()
			{
				Id = instanceId,
				Parent = eo.Get<Guid>("Parent"),
				Workflow = await _workflowStorage.LoadAsync(identity),
				State = _serializer.Deserialize(eo.Get<String>("State")),
				ExecutionStatus = Enum.Parse<WorkflowExecutionStatus>(eo.Get<String>("ExecutionStatus")),
				Lock = eo.Get<Guid>("Lock")
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
			SetIdentityParams(ieo);
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
				{ "Variables", instanceData.ExternalVariables },
				{ "Bookmarks", instanceData.ExternalBookmarks},
				{ "Events", instanceData.ExternalEvents},
				{ "TrackRecords", instanceData.TrackRecords }
			};

			var root = new ExpandoObject()
			{
				{"Instance", ieo }
			};

			List<BatchProcedure> batches = null;
			if (instanceData.Deferred != null)
			{
				batches = new List<BatchProcedure>();
				foreach (var defer in instanceData.Deferred.Where(d => d.Type == DeferredElementType.Sql))
				{
					var epxParam = defer.Parameters.Clone();
					SetIdentityParams(epxParam);
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


		public async Task<IEnumerable<IPendingInstance>> GetPendingAsync()
		{
			return await _dbContext.LoadListAsync<PendingInstance>(null, $"{SqlDefinitions.SqlSchema}.[Instance.Pending.Load]", null);
		}

		#endregion
	}
}
