// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Linq;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	public class TimerEventDefinition : EventDefinition, IScriptable
	{
		TimeBase TimeBase => Children.OfType<TimeBase>().FirstOrDefault();

		public override IWorkflowEvent CreateEvent(String id, IExecutionContext context)
		{
			return new WorkflowTimerEvent(id, NextTriggerTime(context));
		}

		private DateTime NextTriggerTime(IExecutionContext context)
		{
			if (TimeBase == null)
				throw new WorkflowException($"TimerEventDefinition. There is no trigger time for '{Id}'");
			Object timeSpan = TimeBase.Expression;
			if (TimeBase.Expression.IsVariable())
				timeSpan = context.Evaluate<Object>(Id, TimeBaseEvaluate);
			return TimeBase.NextTriggerTime(timeSpan);
		}

		public override Boolean CanRepeat => TimeBase != null && TimeBase.CanRepeat;

		String TimeBaseEvaluate => $"{Id}_TimeBase";

		public void BuildScript(IScriptBuilder builder)
		{
			if (TimeBase == null || !TimeBase.Expression.IsVariable())
				return;
			builder.BuildEvaluate(TimeBaseEvaluate, TimeBase.Expression.Variable());
		}
	}
}
