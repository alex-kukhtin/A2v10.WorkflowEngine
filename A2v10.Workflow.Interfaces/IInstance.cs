// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public enum WorkflowExecutionStatus
{
    Init,
    Idle,
    Complete,
    Faulted
}

public interface IInstance
{
    IWorkflow Workflow { get; }

    Guid Id { get; }
    Guid? Parent { get; }

    WorkflowExecutionStatus ExecutionStatus { get; set; }
    Guid? Lock { get; }

    String? CorrelationId { get; set; }

    ExpandoObject? Result { get; set; }
    ExpandoObject? State { get; set; }

    IInstanceData? InstanceData { get; set; }

    List<ExpandoObject>? Signal { get; set; }
}

