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

		const String WorkflowIdProperty = "WorkflowId";
		const String InstanceIdProperty = "InstanceId";
		const String ArgsProperty = "Args";
		const String ReplyProperty = "Reply";
		const String ResultProperty = "Result";

		public WorkflowInvokeTarget(IWorkflowEngine engine, IWorkflowStorage storage)
		{
			_engine = engine;
			_storage = storage;
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
				{ InstanceIdProperty, res.Id }
			};
		}

		public async Task<ExpandoObject> RunAsync(Guid instanceId, ExpandoObject args)
		{
			var res = await _engine.RunAsync(instanceId, args);
			return new ExpandoObject()
			{
				{ InstanceIdProperty, res.Id },
				{ ResultProperty, res.Result}
			};
		}

		public async Task<ExpandoObject> ResumeAsync(Guid instanceId, String bookmark, Object reply)
		{
			var res = await _engine.ResumeAsync(instanceId, bookmark, reply);
			return new ExpandoObject()
			{
				{ InstanceIdProperty, res.Id },
				{ ResultProperty, res.Result}
			};
		}

		public async Task<ExpandoObject> StartAsync(String workflowId, Int32 version, ExpandoObject args)
		{
			var resCreate = await CreateAsync(workflowId, version);
			var instId = resCreate.Get<Guid>(InstanceIdProperty);
			return await RunAsync(instId, args);
		}

		public async Task<ExpandoObject> InvokeAsync(String method, ExpandoObject parameters)
		{
			return method switch
			{
				"Create" => await CreateAsync(
					parameters.Get<String>(WorkflowIdProperty)
				),
				"Run" => await RunAsync(
					parameters.Get<Guid>(InstanceIdProperty),
					parameters.Get<ExpandoObject>(ArgsProperty)
				),
				"Resume" => await ResumeAsync(
					parameters.Get<Guid>(InstanceIdProperty),
					parameters.Get<String>("Bookmark"),
					parameters.Get<ExpandoObject>(ReplyProperty)
				),
				"Start" => await StartAsync(
					parameters.Get<String>(WorkflowIdProperty),
					parameters.Get<Int32>("Version"),
					parameters.Get<ExpandoObject>(ArgsProperty)
				),
				_ => throw new WorkflowException($"Invalid target method '{method}'")
			};
		}
	}
}
