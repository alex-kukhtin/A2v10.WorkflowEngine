// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.


namespace A2v10.Workflow.Bpmn;

public class EscalationEventDefinition : EventDefinition
{
    public String? EscalationRef { get; init; }
    public String? EscalationCodeVariable { get; init; }

    public override IWorkflowEvent CreateEvent(string id, IExecutionContext context)
    {
        return new WorkflowEscalationEvent(id, EscalationRef ?? throw new InvalidProgramException("EscalationRef is null"));
    }
}

