// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.SqlServer.Tests
{
	public class AppScriptNativeObjects : IScriptNativeObjectProvider
	{
		private readonly NativeType[] _nativeTypes = new NativeType[] {
			new NativeType() {Name = "Database", Type = typeof(ScriptNativeDatabase)},
			new NativeType() {Name = "Deferred", Type = typeof(ScriptNativeDeferred)}
		};

		public IEnumerable<NativeType> NativeTypes()
		{
			return _nativeTypes;
		}
	}
}
