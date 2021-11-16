// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	public class EndEvent : Event
	{
		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token, ExecutingAction onComplete)
		{
			if (!String.IsNullOrEmpty(Script))
				context.Execute(Id, nameof(Script));

			Parent.KillToken(token);
			Parent.TryComplete(context);
			return ValueTask.CompletedTask;
		}
	}
}
