// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
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
}
