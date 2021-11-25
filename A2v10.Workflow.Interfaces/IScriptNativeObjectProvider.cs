// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;

public record NativeType(Type Type, String Name);

public interface IScriptNativeObjectProvider
{
	IEnumerable<NativeType> NativeTypes();
}

