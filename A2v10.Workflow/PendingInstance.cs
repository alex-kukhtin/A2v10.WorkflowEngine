// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.


using System.Collections.Generic;

namespace A2v10.Workflow;
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

