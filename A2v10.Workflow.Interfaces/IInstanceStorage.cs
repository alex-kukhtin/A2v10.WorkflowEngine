// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;

public record PendingElement(
    IEnumerable<IPendingInstance> Pending, 
    IEnumerable<IAutoStartInstance> AutoStart,
    IEnumerable<IPendingMessage> Messages
);

public interface IInstanceStorage
{
    Task<IInstance> Load(Guid id);
    Task<IInstance> LoadRaw(Guid id);

    Task Create(IInstance instance);
    Task Save(IInstance instance);

    Task WriteException(Guid id, Exception ex);

    Task<PendingElement?> GetPendingAsync();
    Task AutoStartComplete(Int64 Id, Guid instanceId);
    Task PendingMessageComplete(Int64 Id, Guid instanceId);
    Task<IInstance?> LoadBookmark(String bookmark);
    Task CancelChildren(Guid id, String workflow);
    ValueTask<DateTime> GetNowTime();
    ExpandoObject LoadPersistentValue(String procedure, Object id);
    ExpandoObject SavePersistentValue(String procedure, ExpandoObject obj);
    Task SetPersistentInstanceAsync(String procedure, String correlationId, Guid instanceId, String workflowId);
}

