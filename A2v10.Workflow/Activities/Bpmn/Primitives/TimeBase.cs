// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using A2v10.System.Xaml;

namespace A2v10.Workflow.Bpmn
{
	[ContentProperty("Expression")]
	public abstract class TimeBase : BaseElement
	{
		public String Type { get; init; }
		public String Expression { get; init; }

		public abstract Boolean CanRepeat { get; }
		public abstract DateTime NextTriggerTime(Object arg);
	}
}
