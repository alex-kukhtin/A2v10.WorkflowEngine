﻿// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Workflow.Bpmn
{
	public class TimeCycle : TimeBase
	{
		public override Boolean CanRepeat => true;
		public override DateTime NextTriggerTime => DateTime.UtcNow + TimeSpan.Parse(Expression);
	}
}