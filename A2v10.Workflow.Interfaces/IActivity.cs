// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace A2v10.Workflow.Interfaces
{
	public interface IActivity
	{
		String Id { get; init;  }
		IActivity Parent { get; }

		ValueTask ExecuteAsync(IExecutionContext context, IToken token);
		void Cancel(IExecutionContext context);
		void TryComplete(IExecutionContext context, IActivity activity);

		IEnumerable<IActivity> EnumChildren();
		void OnEndInit(IActivity parent);

	}
}
