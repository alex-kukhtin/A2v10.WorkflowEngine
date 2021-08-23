// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace A2v10.Workflow.Interfaces
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	public abstract class Activity : IActivity
	{
		#region IActivity
		public String Id { get; set; }

		public abstract ValueTask ExecuteAsync(IExecutionContext context, IToken token, ExecutingAction onComplete);

		public virtual IEnumerable<IActivity> EnumChildren()
		{
			return Enumerable.Empty<IActivity>();
		}

		public void Cancel(IExecutionContext context)
		{
		}

		public virtual void OnEndInit()
		{
			foreach (var act in EnumChildren())
				act.OnEndInit();
		}
		#endregion
	}
}
