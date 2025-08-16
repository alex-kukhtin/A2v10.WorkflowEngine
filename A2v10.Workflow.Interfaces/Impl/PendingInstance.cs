// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public class PendingInstance : IPendingInstance
{
    private readonly List<String> _eventKeys = [];

    #region IPendingInstance
    public Guid InstanceId { get; set; }
    public IEnumerable<String> EventKeys => _eventKeys;
    #endregion

    public void AddEventKey(String key)
    {
        _eventKeys.Add(key);
    }
}

public class MessageInstance : IPendingMessage
{
    public Int64 Id { get; init; }
    public Guid InstanceId { get; init; }
    public String Message { get; init; } = default!;
}
