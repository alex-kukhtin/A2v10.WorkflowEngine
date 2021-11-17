// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	public abstract class ActivityWithComplete : Activity, IStorable
	{
		protected IToken _token;

		#region IStorable
		const String TOKEN = "Token";

		public void Store(IActivityStorage storage)
		{
			storage.SetToken(TOKEN, _token);
			OnStore(storage);
		}

		public void Restore(IActivityStorage storage)
		{
			_token = storage.GetToken(TOKEN);
			OnRestore(storage);
		}
		#endregion

		public virtual void OnStore(IActivityStorage storage)
		{
		}

		public virtual void OnRestore(IActivityStorage storage)
		{
		}
	}
}
