// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	public class FlowDecision : FlowNode, IScriptable
	{
		public String Condition { get; set; }
		public String Then { get; set; }
		public String Else { get; set; }

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token, ExecutingAction onComplete)
		{
			var cond = context.Evaluate<Boolean>(Id, nameof(Condition));
			var nextNode = Parent.FindNode(cond ? Then : Else);
			if (nextNode == null)
				nextNode = Parent.FindNode(Next);
			if (nextNode != null)
				context.Schedule(nextNode, onComplete, token);
			return ValueTask.CompletedTask;
		}

		#region IScriptable
		public void BuildScript(IScriptBuilder builder)
		{
			builder.BuildEvaluate(nameof(Condition), Condition);
		}
		#endregion
	}
}
