// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Workflow.Bpmn
{
	public class TimeDate : TimeBase
	{
		public override Boolean CanRepeat => false;
		public override DateTime NextTriggerTime => throw new NotImplementedException("TimeDate.NextTriggerTime");
	}
}
