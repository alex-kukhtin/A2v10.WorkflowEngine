// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

namespace A2v10.Workflow.Interfaces
{
	public record NativeType
	{
		public Type Type { get; init; }
		public String Name { get; init; }
	}

	public interface IScriptNativeObjectProvider
	{
		IEnumerable<NativeType> NativeTypes();
	}
}
