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

		protected ProcessBase Parent { get; private set; }

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

		public abstract ValueTask ExecuteAsync(IExecutionContext context, IToken token, ExecutingAction onComplete);

		public virtual void OnEndInit()
		{
			foreach (var act in EnumChildren())
				act.OnEndInit();
		}
		#endregion

		public void SetParent(ProcessBase parent)
		{
			Parent = parent;
		}
	}
}
