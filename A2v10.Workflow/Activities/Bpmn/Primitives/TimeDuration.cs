// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Workflow.Bpmn
{
	public class TimeDuration : TimeBase
	{
		public override Boolean CanRepeat => false;

		public override DateTime NextTriggerTime(String arg) =>
			DateTime.UtcNow + TimeSpan.Parse(arg);
	}
}
