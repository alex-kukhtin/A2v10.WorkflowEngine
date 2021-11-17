// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	public class IntermediateThrowEvent : Event
	{
		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token)
		{
			throw new NotImplementedException();
		}
	}
}
