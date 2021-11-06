// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Linq;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	public class UserTask : BpmnTask
	{
		// wf:Script here
		public String Script => ExtensionElements<A2v10.Workflow.Script>()?.FirstOrDefault()?.Text;

		protected override bool CanInduceIdle => true;

		public override ValueTask ExecuteBody(IExecutionContext context)
		{
			context.SetBookmark(Id, this, OnUserTaskComplete);
			return ValueTask.CompletedTask;
		}

		[StoreName("OnUserTaskComplete")]
		ValueTask OnUserTaskComplete(IExecutionContext context, String bookmark, Object result)
		{
			CompleteTask(context);
			context.RemoveBookmark(bookmark);
			if (!String.IsNullOrEmpty(Script))
				context.ExecuteResult(Id, nameof(Script), result);
			return CompleteBody(context);
		}

		public override void BuildScriptBody(IScriptBuilder builder)
		{
			builder.BuildExecuteResult(nameof(Script), Script);
		}
	}
}
