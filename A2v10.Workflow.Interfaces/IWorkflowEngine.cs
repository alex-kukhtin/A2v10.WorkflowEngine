// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.


namespace A2v10.Workflow.Interfaces;
public interface IWorkflowEngine
{
	ValueTask<IInstance> CreateAsync(IWorkflowIdentity identity, Guid? parent = null);
	ValueTask<IInstance> CreateAsync(IActivity root, IWorkflowIdentity? identity, Guid? parent = null);

	ValueTask<IInstance> RunAsync(Guid id, Object? args = null);
	ValueTask<IInstance> RunAsync(IInstance instance, Object? args = null);

	ValueTask<IInstance> ResumeAsync(Guid id, String bookmark, Object? reply = null);
	ValueTask<IInstance> HandleEventAsync(Guid id, String eventKey, Object? reply = null);

	ValueTask<IInstance> LoadInstance(Guid id);

	ValueTask ProcessPending();
}

