// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.System.Xaml;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	[ContentProperty("Expression")]
	public abstract class TimeBase : BaseElement
	{
		public String Type { get; init; }
		public String Expression { get; init; }

		public abstract Boolean CanRepeat { get; }
		public abstract DateTime NextTriggerTime(String arg);


		public String ActualExpression(IExecutionContext context)
		{
			// TODO: убрать отсюда
			if (Expression.IsVariable())
				return context.Evaluate<String>(nameof(Expression), Id);
			return Expression;
		}
	}
}
