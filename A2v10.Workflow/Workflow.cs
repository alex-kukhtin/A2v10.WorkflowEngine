// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.


namespace A2v10.Workflow;
public class Workflow : IWorkflow
{
	public Workflow(IWorkflowIdentity identity, IActivity root, IActivityWrapper wrapper)
    {
		Identity = identity;
		Root = root;
		Wrapper = wrapper;
    }
	public Workflow(IWorkflowIdentity identity, DeserializeResult deserializeResult)
	{
		Identity = identity;
        Root = deserializeResult.Activity;
		Wrapper = deserializeResult.Wrapper;
	}
    public IWorkflowIdentity Identity { get; init; }
	public IActivity Root { get; init; }
	public IActivityWrapper? Wrapper { get; init; }
}
