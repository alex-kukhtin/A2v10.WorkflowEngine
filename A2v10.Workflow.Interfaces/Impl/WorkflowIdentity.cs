// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public record WorkflowIdentity : IWorkflowIdentity
{
    public WorkflowIdentity(String id, Int32 ver = 0)
    {
        Id = id;
        Version = ver;
    }
    public String Id { get; }
    public Int32 Version { get; }
}
