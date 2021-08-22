// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

using A2v10.System.Xaml;

namespace A2v10.Workflow.Bpmn
{
	[ContentProperty("Text")]
	public class GlobalScript : BaseElement
	{
		public String Text { get; init; }
	}
}
