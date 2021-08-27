// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	public class StartState : StateBase
	{
		public override Boolean IsStart => true;

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token, ExecutingAction onComplete)
		{
			NextState = Next;
			if (onComplete != null)
				return onComplete(context, this);
			return ValueTask.CompletedTask;
		}
	}
}
