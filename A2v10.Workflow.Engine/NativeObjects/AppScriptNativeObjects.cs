// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;
using System.Linq;

using A2v10.Workflow;
using A2v10.Workflow.Interfaces;
using A2v10.Workflow.SqlServer;

namespace A2v10.WorkflowEngine;
public class AppScriptNativeObjects : IScriptNativeObjectProvider
{
	private readonly NativeType[] _nativeTypes = new NativeType[] {
		new NativeType(Name:"Database", Type:typeof(ScriptNativeDatabase)),
		new NativeType(Name:"Deferred", Type:typeof(ScriptNativeDeferred))
	};

	private readonly IEnumerable<NativeType> _customTypes;

	public AppScriptNativeObjects(IEnumerable<NativeType>? customTypes)
    {
		_customTypes = customTypes ?? Enumerable.Empty<NativeType>();
	}

	public IEnumerable<NativeType> NativeTypes()
	{
		return Enumerable.Concat(_nativeTypes, _customTypes);
	}
}

