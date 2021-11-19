// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using System;

namespace A2v10.Workflow.Bpmn
{
	public class TimeCycle : TimeBase
	{
		public override Boolean CanRepeat => true;
		public override DateTime NextTriggerTime(String span)
			=> DateTime.UtcNow + TimeSpan.Parse(span);
	}
}
