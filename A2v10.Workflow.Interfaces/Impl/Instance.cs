// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;

public class Instance(IWorkflow workflow, Guid id, String? correlationId = null, Guid? parent = null) : IInstance
{
    public IWorkflow Workflow { get; init; } = workflow;
    public Guid Id { get; init; } = id;
    public Guid? Parent { get; init; } = parent;

    public WorkflowExecutionStatus ExecutionStatus { get; set; }
    public Guid? Lock { get; init; }

    public ExpandoObject? Result { get; set; }
    public ExpandoObject? State { get; set; }

    public String? CorrelationId { get; set; } = correlationId;
    public IInstanceData? InstanceData { get; set; }
    public List<ExpandoObject>? Signal { get; set; }
}
