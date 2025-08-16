// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;

namespace A2v10.Workflow;

public class ScriptNativeDeferred : IInjectable
{
    private IDeferredTarget? _deferredTarget;

    public void Inject(IServiceProvider serviceProvider)
    {
    }

    public void SetDeferred(IDeferredTarget deferredTarget)
    {
        _deferredTarget = deferredTarget;
    }

#pragma warning disable IDE1006 // Naming Styles
    public void executeSql(String procedure, ExpandoObject? prms = null)
#pragma warning restore IDE1006 // Naming Styles
    {
        if (_deferredTarget == null)
            throw new InvalidProgramException("DeferredTarget is null");
        _deferredTarget.AddDeffered(new DeferredElement(DeferredElementType.Sql, procedure, prms, _deferredTarget.Refer));
    }
}
