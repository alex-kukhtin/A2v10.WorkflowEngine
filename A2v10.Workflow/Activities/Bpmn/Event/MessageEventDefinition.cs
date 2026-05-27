// Copyright © 2020-2026 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Bpmn;
public class MessageEventDefinition : EventDefinition
{
    public String? MessageRef { get; init; }
    public override ValueTask<IWorkflowEvent> CreateEvent(String id, IExecutionContext context)
    {
        var evt = new WorkflowMessageEvent(id, MessageRef ?? throw new WorkflowException("MessageRef is null"));
        return ValueTask.FromResult<IWorkflowEvent>(evt);
    }
}

