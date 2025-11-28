// Copyright © 2020-2022 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;

namespace A2v10.Workflow.Bpmn;

public class StartEvent : Event
{

    public override Boolean IsStart => true;

    public override async ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
    {
        var eventDef = EventDefinition;
        if (eventDef != null)
            context.AddEvent(await eventDef.CreateEvent(Id, context), this, OnTrigger);
        else
            Complete(context);
    }

    [StoreName("OnTrigger")]
    public ValueTask OnTrigger(IExecutionContext context, IWorkflowEvent wfEvent, Object? result)
    {
        SetComplete(context);
        Complete(context);
        return ValueTask.CompletedTask;
    }

    void Complete(IExecutionContext context)
	{
        if (!String.IsNullOrEmpty(Script))
            context.Execute(Id, nameof(Script));
        if (Outgoing == null)
            return;
        var track = context.Evaluate<ExpandoObject>(Id, nameof(Track));
        context.AddTrack(track, this);

        foreach (var flow in Outgoing)
        {
            var flowElem = ParentContainer.FindElement<SequenceFlow>(flow.Text);
            if (flowElem.SourceRef != Id)
                throw new WorkflowException($"BPMN. Invalid SequenceFlow (Id={Id}. SourceRef does not match");
            // generate new token for every outogoing flow!
            context.Schedule(flowElem, ParentContainer.NewToken());
        }
    }
}