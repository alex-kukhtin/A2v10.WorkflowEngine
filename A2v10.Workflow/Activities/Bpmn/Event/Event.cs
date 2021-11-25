// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

using A2v10.System.Xaml;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	[ContentProperty("Children")]
	public abstract class Event : BpmnActivity, IScriptable
	{
		public virtual Boolean IsStart => false;

		public IEnumerable<Outgoing>? Outgoing => Children?.OfType<Outgoing>();

		public EventDefinition? EventDefinition => Children?.OfType<EventDefinition>().FirstOrDefault();


		// wf:Script here
		public String? Script => ExtensionElements<A2v10.Workflow.Script>()?.FirstOrDefault()?.Text;

		#region IScriptable
		public void BuildScript(IScriptBuilder builder)
		{
			builder.BuildExecute(nameof(Script), Script);
		}
		#endregion

		protected void ScheduleOutgoing(IExecutionContext context, IToken? token)
		{
			if (Outgoing == null)
				return;
			if (ParentContainer == null)
				throw new WorkflowException("ParentContainer is null");
			if (Outgoing.Count() == 1)
			{
				var targetFlow = ParentContainer.FindElement<SequenceFlow>(Outgoing.First().Text);
				context.Schedule(targetFlow, token);
			}
			else
			{
				ParentContainer.KillToken(token);
				foreach (var o in Outgoing)
				{
					var targetFlow = ParentContainer.FindElement<SequenceFlow>(o.Text);
					context.Schedule(targetFlow, ParentContainer.NewToken());
				}
			}
		}

		public override void Cancel(IExecutionContext context)
		{
			SetComplete(context);
		}

		protected void SetComplete(IExecutionContext context)
		{
			context.RemoveEvent(Id);
		}

		public override IEnumerable<IActivity> EnumChildren()
		{
			if (EventDefinition != null)
				yield return EventDefinition;
		}
	}
}
