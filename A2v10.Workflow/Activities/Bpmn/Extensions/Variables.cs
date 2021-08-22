// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;

using A2v10.System.Xaml;

namespace A2v10.Workflow.Bpmn
{
	[ContentProperty("Items")]
	public class Variables : BaseElement
	{
		public List<Variable> Items { get; init; }
	}
}
