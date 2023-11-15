// Copyright © 2023 Oleksandr Kukhtin. All rights reserved.

using A2v10.Workflow;
using A2v10.Workflow.Interfaces;
using A2v10.Workflow.SqlServer;
using System.Collections.Generic;
using System.Linq;

namespace A2v10.WorkflowEngine;
public class AppScriptNativeObjects(IEnumerable<NativeType>? customTypes) : IScriptNativeObjectProvider
{
    private readonly NativeType[] _nativeTypes = [
        new(Name:"Database", Type:typeof(ScriptNativeDatabase)),
        new(Name:"Deferred", Type:typeof(ScriptNativeDeferred))
    ];

    private readonly IEnumerable<NativeType> _customTypes = customTypes ?? Enumerable.Empty<NativeType>();

    public IEnumerable<NativeType> NativeTypes()
    {
        return Enumerable.Concat(_nativeTypes, _customTypes);
    }
}

