﻿// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.Workflow.Interfaces
{
	public interface IWorkflowEngine
	{
		ValueTask<IInstance> CreateAsync(IWorkflowIdentity identity);
		ValueTask<IInstance> CreateAsync(IActivity root, IWorkflowIdentity identity);

		ValueTask<IInstance> RunAsync(Guid id, Object args = null);
		ValueTask<IInstance> RunAsync(IInstance instance, Object args = null);

		ValueTask<IInstance> ResumeAsync(Guid id, String bookmark, Object reply = null);
		ValueTask<IInstance> HandleEventAsync(Guid id, String eventKey, Object reply = null);

		ValueTask ProcessPending();
	}
}