// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace A2v10.Workflow.Interfaces
{
	public abstract class Activity : IActivity
	{
		#region IActivity
		public String Id { get; init; }
		public IActivity Parent { get; private set; }

		public abstract ValueTask ExecuteAsync(IExecutionContext context, IToken token);

		public virtual IEnumerable<IActivity> EnumChildren()
		{
			return Enumerable.Empty<IActivity>();
		}

		public void Cancel(IExecutionContext context)
		{
		}

		public virtual void TryComplete(IExecutionContext context, IActivity activity)
		{
		}

		public virtual void OnEndInit(IActivity parent)
		{
			Parent = parent;
		}


		#endregion
	}
}
