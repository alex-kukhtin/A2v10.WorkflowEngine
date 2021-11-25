// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Workflow.Bpmn
{
	public class CallActivity : BpmnTask
	{
		public String? CalledElement { get; set; }

		protected override bool CanInduceIdle => true;
	}
}
