// Copyright © 2020-2022 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Bpmn;

public class IntermediateCatchEvent : Event, IStorable
{
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

    public override async ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
    {
        _token = token;
        var eventDef = EventDefinition;
        if (eventDef != null)
            context.AddEvent(await eventDef.CreateEvent(Id, context), this, OnTrigger);
        else
            SetComplete(context);
    }

    [StoreName("OnTrigger")]
    public ValueTask OnTrigger(IExecutionContext context, IWorkflowEvent wfEvent, Object? result)
    {
        SetComplete(context);
        ScheduleOutgoing(context, _token);
        return ValueTask.CompletedTask;
    }
}
