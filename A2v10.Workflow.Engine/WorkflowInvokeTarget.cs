// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.Threading.Tasks;

using A2v10.Runtime.Interfaces;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Engine
{
	public class WorkflowInvokeTarget : IRuntimeInvokeTarget
	{
		private readonly IWorkflowEngine _engine;

		const String InstanceIdProperty = "InstanceId";
		const String ResultProperty = "Result";

		public WorkflowInvokeTarget(IWorkflowEngine engine)
		{
			_engine = engine;
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
			var res = await _engine.ResumeAsync(instanceId, bookmark, reply).AsTask();
			return new ExpandoObject()
			{
				{ InstanceIdProperty, res.Id },
				{ ResultProperty, res.Result}
			};
		}

		public async Task<ExpandoObject> InvokeAsync(String method, ExpandoObject parameters)
		{
			return method switch
			{
				"Create" => await CreateAsync(
					parameters.Get<String>("WorkflowId")
				),
				"Run" => await RunAsync(
					parameters.Get<Guid>(InstanceIdProperty),
					parameters.Get<ExpandoObject>("Args")
				),
				"Resume" => await ResumeAsync(
					parameters.Get<Guid>(InstanceIdProperty),
					parameters.Get<String>("Bookmark"),
					parameters.Get<ExpandoObject>("Reply")
				),
				_ => throw new WorkflowException($"Invalid target method '{method}'")
			};
		}
	}
}
