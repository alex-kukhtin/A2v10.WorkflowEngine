// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.


namespace A2v10.Workflow.Interfaces;
public interface IWorkflowEngine
{
	ValueTask<IInstance> CreateAsync(IWorkflowIdentity identity, Guid? parent = null);
	ValueTask<IInstance> CreateAsync(IActivity root, IWorkflowIdentity? identity, Guid? parent = null);
	ValueTask<IInstance> CreateAsync(IWorkflow workflow, Guid? parent = null);

	ValueTask<IInstance> RunAsync(Guid id, Object? args = null);
	ValueTask<IInstance> RunAsync(IInstance instance, Object? args = null);

	ValueTask<IInstance> ResumeAsync(Guid id, String bookmark, Object? reply = null);
	ValueTask<IInstance> HandleEventsAsync(Guid id, IEnumerable<String> eventKeys);
	ValueTask<IInstance> SendMessageAsync(Guid id, String message);

	ValueTask<IInstance> LoadInstanceRaw(Guid id);

	ValueTask ProcessPending();
}

