// Copyright © 2022 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;


public record WorkflowStorageVersion(Boolean Valid, Int32 Required, Int32 Actual);

public interface IWorkflowStorageVersion
{
	WorkflowStorageVersion GetVersion();
}
