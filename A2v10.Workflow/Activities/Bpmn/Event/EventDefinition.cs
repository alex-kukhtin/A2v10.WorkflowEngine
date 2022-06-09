// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.


namespace A2v10.Workflow.Bpmn;
public class EventDefinition : BpmnActivity
{
    public virtual ValueTask<IWorkflowEvent> CreateEvent(String id, IExecutionContext context)
    {
        throw new NotImplementedException();
    }

    public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
    {
        throw new NotImplementedException();
    }

    public virtual Boolean CanRepeat { get; }
}

