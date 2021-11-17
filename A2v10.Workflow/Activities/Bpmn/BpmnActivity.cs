// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	public abstract class BpmnActivity : BaseElement, IActivity
	{
		public String Name { get; init; }

		public IContainer Parent { get; private set; }

		IActivity IActivity.Parent => Parent;

		#region IActivity

		public virtual void Cancel(IExecutionContext context)
		{
			foreach (var ch in EnumChildren())
				ch.Cancel(context);
		}

		public virtual IEnumerable<IActivity> EnumChildren()
		{
			return Enumerable.Empty<IActivity>();
		}

		public abstract ValueTask ExecuteAsync(IExecutionContext context, IToken token);

		public virtual void OnEndInit(IActivity parent)
		{
			if (parent == null)
				return;
			if (parent is IContainer cont)
				Parent = cont;
			else
				throw new WorkflowException("Parent activity is not a container");
		}

		public virtual void TryComplete(IExecutionContext context, IActivity activity)
		{
			Parent?.TryComplete(context, this);
		}
		#endregion
	}
}
