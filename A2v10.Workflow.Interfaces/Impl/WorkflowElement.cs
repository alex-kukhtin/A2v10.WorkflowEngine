// Copyright © 2020-2022 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public class WorkflowElement : IWorkflow
{
    public WorkflowElement(IWorkflowIdentity identity, IActivity root, IActivityWrapper wrapper)
    {
        Identity = identity;
        Root = root;
        Wrapper = wrapper;
    }
    public WorkflowElement(IWorkflowIdentity identity, DeserializeResult deserializeResult)
    {
        Identity = identity;
        Root = deserializeResult.Activity;
        Wrapper = deserializeResult.Wrapper;
    }
    public IWorkflowIdentity Identity { get; init; }
    public IActivity Root { get; init; }
    public IActivityWrapper? Wrapper { get; init; }
}
