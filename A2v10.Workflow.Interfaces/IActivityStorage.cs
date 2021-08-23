// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace A2v10.Workflow.Interfaces
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	public interface IActivityStorage
	{
		Boolean IsLoading { get; }
		Boolean IsStoring { get; }

		void Set<T>(String name, T value);
		void SetCallback(String name, ExecutingAction callback);
		void SetToken(String name, IToken value);
		void SetTokenList(String name, List<IToken> list);

		ExecutingAction GetCallback(String name);
		T Get<T>(String name);
		IToken GetToken(String name);
		void GetTokenList(String name, List<IToken> list);
	}
}
