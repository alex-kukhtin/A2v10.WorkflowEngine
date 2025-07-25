// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.Workflow.Interfaces;

public interface ISyntaxError
{
    String Script { get; }
    String Message { get; }
    String? ActivityId { get; }
}

public interface IWorkflowEngine
{
    ValueTask<IInstance> CreateAsync(IWorkflowIdentity identity, String? correlationId = null, Guid? parent = null, Guid? instanceId = null);
    ValueTask<IInstance> CreateAsync(IActivity root, IWorkflowIdentity? identity, String? correlationId = null, Guid? parent = null, Guid? instanceId = null);
    ValueTask<IInstance> CreateAsync(IWorkflow workflow, String? correlationId = null, Guid? parent = null, Guid? instanceId = null);

    ValueTask<IInstance> RunAsync(Guid id, Object? args = null, IToken? token = null);
    ValueTask<IInstance> RunAsync(IInstance instance, Object? args = null, IToken? token = null);

    ValueTask<IInstance> ResumeAsync(Guid id, String bookmark, Object? reply = null);
    ValueTask<IInstance?> HandleEventsAsync(Guid id, IEnumerable<String> eventKeys);
    ValueTask<IInstance> SendMessageAsync(Guid id, String message);

    ValueTask<IInstance> LoadInstanceRaw(Guid id);
    Task CancelChildren(Guid id, String workflow);

    ValueTask ProcessPending();
}

