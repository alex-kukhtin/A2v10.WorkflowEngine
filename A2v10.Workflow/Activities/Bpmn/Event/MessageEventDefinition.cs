// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn;
public class MessageEventDefinition : EventDefinition
{
    public String? MessageRef { get; init; }
    public override IWorkflowEvent CreateEvent(String id, IExecutionContext context)
    {
        return new WorkflowMessageEvent(id, MessageRef ?? throw new InvalidProgramException("MessageRef is null"));
    }
}

