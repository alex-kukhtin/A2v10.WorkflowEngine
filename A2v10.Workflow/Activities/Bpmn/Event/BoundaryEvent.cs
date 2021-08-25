// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	public class BoundaryEvent : Event
	{
		public String AttachedToRef { get; init; }
		public Boolean? CancelActivity { get; init; } // default is true!

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token, ExecutingAction onComplete)
		{
			_onComplete = onComplete;
			_token = token;
			var eventDef = EventDefinition;
			if (eventDef != null)
				context.AddEvent(eventDef.CreateEvent(Id), this, OnTrigger);
			else
				SetComplete(context);
			return ValueTask.CompletedTask;
		}

		[StoreName("OnTrigger")]
		public ValueTask OnTrigger(IExecutionContext context, IWorkflowEvent wfEvent, Object result)
		{
			ScheduleOutgoing(context);
			if (CancelActivity == null || CancelActivity.Value)
			{
				var task = Parent.FindElement<BpmnTask>(AttachedToRef);
				SetComplete(context);
				task.Cancel(context);
			}
			else
			{
				var eventDef = EventDefinition;
				if (eventDef != null && eventDef.CanRepeat)
					context.AddEvent(eventDef.CreateEvent(Id), this, OnTrigger);
				else
					SetComplete(context);
			}
			return ValueTask.CompletedTask;
		}
	}

}
