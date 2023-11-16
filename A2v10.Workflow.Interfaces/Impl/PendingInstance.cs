// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

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
