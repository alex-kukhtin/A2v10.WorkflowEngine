// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.


using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	public class BoundaryEvent : Event, IStorable
	{
		public String AttachedToRef { get; init; } = String.Empty;
		public Boolean? CancelActivity { get; init; } // default is true!

		protected IToken? _token;

		#region IStorable
		const String TOKEN = "Token";

		public void Store(IActivityStorage storage)
		{
			storage.SetToken(TOKEN, _token);
		}

		public void Restore(IActivityStorage storage)
		{
			_token = storage.GetToken(TOKEN);
		}
		#endregion

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
		{
			_token = token;
			var eventDef = EventDefinition;
			if (eventDef != null)
				context.AddEvent(eventDef.CreateEvent(Id, context), this, OnTrigger);
			else
				SetComplete(context);
			return ValueTask.CompletedTask;
		}

		[StoreName("OnTrigger")]
		public ValueTask OnTrigger(IExecutionContext context, IWorkflowEvent wfEvent, Object? result)
		{
			if (ParentContainer == null)
				throw new InvalidProgramException("Invalid ParentContainer");
			ScheduleOutgoing(context, _token);
			if (CancelActivity == null || CancelActivity.Value)
			{
				var cancelable = ParentContainer.FindElement<BpmnActivity>(AttachedToRef);
				SetComplete(context);
				cancelable.Cancel(context);
			}
			else
			{
				var eventDef = EventDefinition;
				if (eventDef != null && eventDef.CanRepeat)
					context.AddEvent(eventDef.CreateEvent(Id, context), this, OnTrigger);
				else
					SetComplete(context);
			}
			return ValueTask.CompletedTask;
		}
	}

}
