// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public sealed class WorkflowException : Exception
{
	public WorkflowException(String message)
		: base(message)
	{
	}
}
