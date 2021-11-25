// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	public class Workflow : IWorkflow
	{
		public Workflow(IWorkflowIdentity identity, IActivity root)
        {
			Identity = identity;
			Root = root;
        }
		public IWorkflowIdentity Identity { get; init; }
		public IActivity Root { get; init; }
	}
}
