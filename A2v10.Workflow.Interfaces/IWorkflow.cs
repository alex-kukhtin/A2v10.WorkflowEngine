﻿// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces
{
	public interface IWorkflow
	{
		IWorkflowIdentity Identity { get; }
		IActivity Root { get; }
	}
}