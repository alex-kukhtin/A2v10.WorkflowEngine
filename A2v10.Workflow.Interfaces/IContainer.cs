// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

namespace A2v10.Workflow.Interfaces
{
	public interface IContainer
	{
		IToken NewToken();
		void KillToken(IToken token);

		void TryComplete(IExecutionContext context);

		void OnEndInit();

		T FindElement<T>(String id);
		IEnumerable<T> FindAll<T>(Predicate<T> predicate);
	}
}
