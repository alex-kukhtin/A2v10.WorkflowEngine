// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

using A2v10.System.Xaml;

namespace A2v10.Workflow.Bpmn
{
	[ContentProperty("Children")]
	public abstract class FlowElement : BpmnActivity
	{
		public String Default { get; init; }

		public Boolean HasIncoming => Children != null && Children.OfType<Incoming>().Any();
		public Boolean HasOutgoing => Children != null && Children.OfType<Outgoing>().Any();

		internal IEnumerable<Incoming> Incoming => Children?.OfType<Incoming>();
		internal IEnumerable<Outgoing> Outgoing => Children?.OfType<Outgoing>();
	}
}
