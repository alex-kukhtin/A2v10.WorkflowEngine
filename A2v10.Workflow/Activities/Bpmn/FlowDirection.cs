// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

using A2v10.System.Xaml;

namespace A2v10.Workflow.Bpmn
{
	[ContentProperty("Text")]
	public abstract class FlowDirection : BaseElement
	{
		public String Text { get; set; }
		public abstract Boolean IsIncoming { get; }
	}

	public class Incoming : FlowDirection
	{
		public override Boolean IsIncoming => true;
	}

	public class Outgoing : FlowDirection
	{
		public override Boolean IsIncoming => false;
	}
}
