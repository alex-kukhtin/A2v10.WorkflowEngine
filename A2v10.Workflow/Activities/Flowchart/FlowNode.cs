// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	public abstract class FlowNode : Activity
	{
		public virtual Boolean IsStart => false;

		public String Next { get; set; }

		internal Flowchart ParentFlow => Parent as Flowchart;
	}
}
