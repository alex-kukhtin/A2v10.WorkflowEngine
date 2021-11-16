// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

using A2v10.System.Xaml;
using A2v10.Workflow.Interfaces;

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

		public virtual void DoOutgoing(IExecutionContext context, IToken token)
		{
			if (!HasOutgoing)
			{
				Parent.TryComplete(context);
				return;
			}
			var cnt = Outgoing.Count();
			if (cnt == 1)
			{
				// simple outgouning - same token
				var targetFlow = Parent.FindElement<SequenceFlow>(Outgoing.First().Text);
				context.Schedule(targetFlow, null, token);
				return;
			}
			else
			{
				Parent.KillToken(token);
				foreach (var og in Outgoing)
				{
					var flow = Parent.FindElement<SequenceFlow>(og.Text);
					if (flow != null)
					{
						if (flow.Evaluate(context))
							context.Schedule(flow, null, Parent.NewToken());
					}
				}
			}
			Parent.TryComplete(context);
		}
	}
}
