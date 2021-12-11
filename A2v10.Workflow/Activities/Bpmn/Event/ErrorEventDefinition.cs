// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.


namespace A2v10.Workflow.Bpmn;

public class ErrorEventDefinition : EventDefinition
{
    public String? ErrorRef { get; init; }

    public override IWorkflowEvent CreateEvent(string id, IExecutionContext context)
    {
        return new WorkflowErrorEvent(id, ErrorRef ?? throw new InvalidProgramException("ErrorRef is null"));
    }
}

