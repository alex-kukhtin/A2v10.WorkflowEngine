// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.


namespace A2v10.Workflow.Bpmn;

public class EndEvent : Event
{
	public override async ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
	{
		if (!String.IsNullOrEmpty(Script))
			context.Execute(Id, nameof(Script));

		ParentContainer.KillToken(token);

		var ed = EventDefinition;
		if (ed != null)
		{
			var evt = ed.CreateEvent(Id, context);
			await context.HandleEvent(evt);
			context.ProcessEndEvent(evt);
		}

		ParentContainer.TryComplete(context, this);
	}
}

