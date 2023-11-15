// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using System.Collections.Generic;

namespace A2v10.Workflow.SqlServer.Tests;

public class AppScriptNativeObjects : IScriptNativeObjectProvider
{
    private readonly NativeType[] _nativeTypes = [
        new(Name:"Database", Type:typeof(ScriptNativeDatabase)),
        new(Name:"Deferred", Type:typeof(ScriptNativeDeferred))
    ];

    public IEnumerable<NativeType> NativeTypes()
    {
        return _nativeTypes;
    }
}
