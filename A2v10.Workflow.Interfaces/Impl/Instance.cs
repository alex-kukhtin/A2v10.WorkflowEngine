// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public class Instance : IInstance
{
    public Instance(IWorkflow workflow, Guid id, String? correlationId = null, Guid? parent = null)
    {
        Workflow = workflow;
        Id = id;
        Parent = parent;
        CorrelationId = correlationId;
    }
    public IWorkflow Workflow { get; init; }
    public Guid Id { get; init; }
    public Guid? Parent { get; init; }

    public WorkflowExecutionStatus ExecutionStatus { get; set; }
    public Guid? Lock { get; init; }

    public ExpandoObject? Result { get; set; }
    public ExpandoObject? State { get; set; }

    public String? CorrelationId { get; set; }
    public IInstanceData? InstanceData { get; set; }
}
