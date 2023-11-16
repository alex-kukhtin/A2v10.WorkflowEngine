// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public interface IWorkflowIdentity
{
    String Id { get; }
    Int32 Version { get; }
}

