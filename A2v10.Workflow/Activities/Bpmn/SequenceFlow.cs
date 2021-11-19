// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Linq;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	public class SequenceFlow : BpmnActivity, IScriptable
	{
		public String SourceRef { get; init; }
		public String TargetRef { get; init; }

		public ConditionExpression ConditionExpression => Children?.OfType<ConditionExpression>()?.FirstOrDefault();

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token)
		{
			var target = ParentContainer.FindElement<BpmnActivity>(TargetRef);
			context.Schedule(target, token);
			return ValueTask.CompletedTask;
		}

		#region IScriptable
		public void BuildScript(IScriptBuilder builder)
		{
			var expr = ConditionExpression;
			if (expr != null && !String.IsNullOrEmpty(expr.Expression))
				builder.BuildEvaluate(nameof(ConditionExpression), expr.Expression);
		}
		#endregion

		public Boolean Evaluate(IExecutionContext context)
		{
			var expr = ConditionExpression;
			if (String.IsNullOrEmpty(expr?.Expression))
				return false;
			return context.Evaluate<Boolean>(this.Id, nameof(ConditionExpression));
		}

	}
}
