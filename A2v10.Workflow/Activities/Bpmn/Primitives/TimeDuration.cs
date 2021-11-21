// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Workflow.Bpmn
{
	public class TimeDuration : TimeBase
	{
		public override Boolean CanRepeat => false;

		public override DateTime NextTriggerTime(Object span)
		{
			if (span is String strSpan && TimeSpan.TryParse(strSpan, out TimeSpan timeSpan))
				return DateTime.UtcNow + timeSpan;
			throw new WorkflowException($"TimeDuration.NextTriggerTime can't convert from {span}");
		}
	}
}
