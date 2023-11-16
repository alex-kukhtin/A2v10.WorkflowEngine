// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

using System.Collections.Generic;

namespace A2v10.Workflow;
public class WorkflowDeferred : IDeferredTarget
{
    private readonly Lazy<List<DeferredElement>> _deferred = new();

    #region IDeferredTarget

    public List<DeferredElement>? Deferred => _deferred.IsValueCreated ? _deferred.Value : null;
    public String Refer { get; set; } = String.Empty;

    public void AddDeffered(DeferredElement elem)
    {
        _deferred.Value.Add(elem);
    }
    #endregion
}

