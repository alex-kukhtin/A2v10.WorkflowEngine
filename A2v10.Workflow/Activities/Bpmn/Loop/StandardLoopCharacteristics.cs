﻿// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;
using System.Linq;

namespace A2v10.Workflow.Bpmn
{
	public class StandardLoopCharacteristics : BaseElement
	{
		public Boolean TestBefore { get; init; }
		public Int32 LoopMaximum { get; init; }

		public String LoopCondition => Children?.OfType<LoopCondition>().FirstOrDefault()?.Expression;
	}
}