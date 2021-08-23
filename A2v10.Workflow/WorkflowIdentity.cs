// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	public record WorkflowIdentity : IWorkflowIdentity
	{
		public String Id { get; init; }
		public Int32 Version { get; init; }
	}
}
