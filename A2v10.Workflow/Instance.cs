// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Dynamic;


namespace A2v10.Workflow;
public class Instance : IInstance
{
	public Instance(IWorkflow workflow, Guid id, Guid? parent = null)
    {
		Workflow = workflow;
		Id = id;
		Parent = parent;
    }
	public IWorkflow Workflow { get; init; }
	public Guid Id { get; init; }
	public Guid? Parent { get; init; }

	public WorkflowExecutionStatus ExecutionStatus { get; set; }
	public Guid? Lock { get; init; }

	public ExpandoObject? Result { get; set; }
	public ExpandoObject? State { get; set; }

	public IInstanceData? InstanceData { get; set; }
}

