// Copyright © 2020-2022 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.Workflow.Bpmn;

public class ErrorEventDefinition : EventDefinition
{
    public String? ErrorRef { get; init; }

    public override ValueTask<IWorkflowEvent> CreateEvent(string id, IExecutionContext context)
    {
        var evt = new WorkflowErrorEvent(id, ErrorRef ?? throw new InvalidProgramException("ErrorRef is null"));
        return ValueTask.FromResult<IWorkflowEvent>(evt);
    }
}

