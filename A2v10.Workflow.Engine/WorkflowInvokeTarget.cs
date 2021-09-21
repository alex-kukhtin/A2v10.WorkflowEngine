// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.Threading.Tasks;

using A2v10.Workflow;
using A2v10.Workflow.Interfaces;
using A2v10.Runtime.Interfaces;

namespace A2v10.WorkflowEngine
{
	public class WorkflowInvokeTarget : IRuntimeInvokeTarget
	{
		private readonly IWorkflowEngine _engine;
		private readonly IWorkflowStorage _storage;
		private readonly IWorkflowCatalog _catalog;

		public static class Properties
		{
			public const String WorkflowId = nameof(WorkflowId);
			public const String InstanceId = nameof(InstanceId);
			public const String Args = nameof(Args);
			public const String Bookmark = nameof(Bookmark);
			public const String Reply = nameof(Reply);
			public const String Result = nameof(Result);
			public const String Body = nameof(Body);
			public const String Format = nameof(Format);
			public const String Version = nameof(Version);
		}

		public WorkflowInvokeTarget(IWorkflowEngine engine, IWorkflowStorage storage, IWorkflowCatalog catalog)
		{
			_engine = engine;
			_storage = storage;
			_catalog = catalog;
		}

		public async Task<ExpandoObject> CreateAsync(String workflowId, Int32 version = 0)
		{
			var res = await _engine.CreateAsync(new WorkflowIdentity()
			{
				Id = workflowId,
				Version = version
			});

			return new ExpandoObject()
			{
				{ Properties.InstanceId, res.Id }
			};
		}

		public async Task<ExpandoObject> RunAsync(Guid instanceId, ExpandoObject args)
		{
			var res = await _engine.RunAsync(instanceId, args);
			return new ExpandoObject()
			{
				{ Properties.InstanceId, res.Id },
				{ Properties.Result, res.Result}
			};
		}

		public async Task<ExpandoObject> ResumeAsync(Guid instanceId, String bookmark, Object reply)
		{
			var res = await _engine.ResumeAsync(instanceId, bookmark, reply);
			return new ExpandoObject()
			{
				{ Properties.InstanceId, res.Id },
				{ Properties.Result, res.Result}
			};
		}

		public async Task<ExpandoObject> StartAsync(String workflowId, Int32 version, ExpandoObject args)
		{
			var resCreate = await CreateAsync(workflowId, version);
			var instId = resCreate.Get<Guid>(Properties.InstanceId);
			return await RunAsync(instId, args);
		}

		public async Task<ExpandoObject> SaveAsync(String workflowId, String format, String body)
		{
			await _catalog.SaveAsync(new WorkflowDescriptor()
			{
				Id = workflowId,
				Format = format,
				Body = body
			});
			return new ExpandoObject();
		}

		public async Task<ExpandoObject> PublishAsync(String workflowId)
		{
			var res = await _storage.PublishAsync(_catalog, workflowId);
			return new ExpandoObject()
			{
				{ Properties.WorkflowId, res.Id},
				{ Properties.Version, res.Version }
			};
		}

		public async Task<ExpandoObject> InvokeAsync(String method, ExpandoObject parameters)
		{
			return method switch
			{
				"Create" => await CreateAsync(
					parameters.Get<String>(Properties.WorkflowId)
				),
				"Run" => await RunAsync(
					parameters.Get<Guid>(Properties.InstanceId),
					parameters.Get<ExpandoObject>(Properties.Args)
				),
				"Resume" => await ResumeAsync(
					parameters.Get<Guid>(Properties.InstanceId),
					parameters.Get<String>(Properties.Bookmark),
					parameters.Get<ExpandoObject>(Properties.Reply)
				),
				"Start" => await StartAsync(
					parameters.Get<String>(Properties.WorkflowId),
					parameters.Get<Int32>(Properties.Version),
					parameters.Get<ExpandoObject>(Properties.Args)
				),
				"Save" => await SaveAsync(
					parameters.Get<String>(Properties.WorkflowId),
					parameters.Get<String>(Properties.Format),
					parameters.Get<String>(Properties.Body)
				),
				"Publish" => await PublishAsync(
					parameters.Get<String>(Properties.WorkflowId)
				),
				_ => throw new WorkflowException($"Invalid target method '{method}'. Expected: Save, Publish, Create, Run, Start, Resume")
			};
		}
	}
}
