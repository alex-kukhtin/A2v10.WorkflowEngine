// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public class PendingInstance : IPendingInstance
{
    private readonly List<String> _eventKeys = new();
    #region IPendingInstance
    public Guid InstanceId { get; set; }
    public IEnumerable<String> EventKeys => _eventKeys;
    #endregion

    public void AddEventKey(String key)
    {
        _eventKeys.Add(key);
    }
}
