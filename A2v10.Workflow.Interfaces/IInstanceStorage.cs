// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;

public record PendingElement(IEnumerable<IPendingInstance> Pending, IEnumerable<IAutoStartInstance> AutoStart);

public interface IInstanceStorage
{
	Task<IInstance> Load(Guid id);
	Task<IInstance> LoadRaw(Guid id);

	Task Create(IInstance instance);
	Task Save(IInstance instance);

	Task WriteException(Guid id, Exception ex);

	Task<PendingElement?> GetPendingAsync();
	Task AutoStartComplete(Int64 Id, Guid instanceId);
	Task<IInstance?> LoadBookmark(String bookmark);
}

