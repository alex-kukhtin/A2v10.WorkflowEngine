// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	public class WorkflowEngine : IWorkflowEngine
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly IWorkflowStorage _workflowStorage;
		private readonly IInstanceStorage _instanceStorage;
		private readonly ITracker _tracker;
		private readonly IDeferredTarget _deferredTarget;

		public WorkflowEngine(IServiceProvider serviceProvider, ITracker tracker, IDeferredTarget deferredTarget)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_workflowStorage = _serviceProvider.GetService<IWorkflowStorage>() ?? throw new NullReferenceException("IWorkflowStorage");
			_instanceStorage = _serviceProvider.GetService<IInstanceStorage>() ?? throw new NullReferenceException("IInstanceStorage");
			_deferredTarget = deferredTarget ?? throw new NullReferenceException("IDeferredTarget");
			_tracker = tracker;
		}

		public async ValueTask<IInstance> CreateAsync(IActivity root, IWorkflowIdentity identity)
		{
			var inst = new Instance()
			{
				Id = Guid.NewGuid(),
				Workflow = new Workflow()
				{
					Root = root,
					Identity = identity
				}
			};
			root.OnEndInit(null);
			await _instanceStorage.Create(inst);
			return inst;
		}

		public async ValueTask<IInstance> CreateAsync(IWorkflowIdentity identity)
		{
			var wf = await _workflowStorage.LoadAsync(identity);
			return await CreateAsync(wf.Root, wf.Identity);
		}

		public async ValueTask<IInstance> RunAsync(IInstance instance, Object args = null)
		{
			if (instance.ExecutionStatus != WorkflowExecutionStatus.Init)
				throw new WorkflowException($"Instance (id={instance.Id}) is already running");
			var context = new ExecutionContext(_serviceProvider, _tracker, instance.Workflow.Root, args);
			context.Schedule(instance.Workflow.Root, null);
			await context.RunAsync();
			SetInstanceState(instance, context);
			await _instanceStorage.Save(instance);
			return instance;
		}

		public async ValueTask<IInstance> RunAsync(Guid id, Object args = null)
		{
			try
			{
				IInstance instance = await _instanceStorage.Load(id);
				return await RunAsync(instance, args);
			}
			catch (Exception ex)
			{
				await _instanceStorage.WriteException(id, ex);
				throw;
			}
		}

		public ValueTask<IInstance> ResumeAsync(Guid id, String bookmark, Object reply = null)
		{
			return Handle(id, context => context.ResumeAsync(bookmark, reply));
		}

		public ValueTask<IInstance> HandleEventAsync(Guid id, String eventKey, Object reply = null)
		{
			return Handle(id, context => context.HandleEventAsync(eventKey, reply));
		}

		void SetInstanceState(IInstance inst, ExecutionContext context)
		{
			inst.Result = context.GetResult();
			inst.State = context.GetState();
			inst.ExecutionStatus = context.GetExecutionStatus();
			var instData = new InstanceData()
			{
				ExternalVariables = context.GetExternalVariables(inst.State),
				ExternalBookmarks = context.GetExternalBookmarks(),
				ExternalEvents = context.GetExternalEvents(),
				TrackRecords = context.GetTrackRecords(),
				Deferred = _deferredTarget.Deferred
			};
			inst.InstanceData = instData;
		}

		public async ValueTask ProcessPending()
		{
			foreach (var pi in await _instanceStorage.GetPendingAsync())
				await HandleEventAsync(pi.InstanceId, pi.EventKey);
		}

		private async ValueTask<IInstance> Handle(Guid id, Func<ExecutionContext, ValueTask> action)
		{
			try
			{
				var inst = await _instanceStorage.Load(id);
				inst.Workflow.Root.OnEndInit(null);
				var context = new ExecutionContext(_serviceProvider, _tracker, inst.Workflow.Root);
				context.SetState(inst.State);
				await action(context);
				await context.RunAsync();
				SetInstanceState(inst, context);
				await _instanceStorage.Save(inst);
				return inst;
			}
			catch (Exception ex)
			{
				await _instanceStorage.WriteException(id, ex);
				throw;
			}
		}

	}
}
