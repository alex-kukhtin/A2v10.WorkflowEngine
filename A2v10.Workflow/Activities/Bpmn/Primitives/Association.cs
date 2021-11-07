// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Workflow.Bpmn
{
	public class Association : BaseElement
	{
		public String SourceRef { get; init; }
		public String TargetRef { get; init; }
	}
}
