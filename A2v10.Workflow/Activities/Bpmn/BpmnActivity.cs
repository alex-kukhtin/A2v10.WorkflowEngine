// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;

namespace A2v10.Workflow.Bpmn;
public abstract class BpmnActivity : BaseElement, IActivity, ICancelable
{
	public String? Name { get; init; }

	public IActivity? Parent { get; private set; }

	internal IContainer ParentContainer => Parent as IContainer ?? throw new ArgumentNullException(nameof(ParentContainer));

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

	public abstract ValueTask ExecuteAsync(IExecutionContext context, IToken? token);

	public virtual void OnEndInit(IActivity? parent)
	{
		Parent = parent;
		foreach (var act in EnumChildren())
			act.OnEndInit(this);
	}

	public virtual void TryComplete(IExecutionContext context, IActivity activity)
	{
		Parent?.TryComplete(context, this);
	}
	#endregion

	// newtonsoft support
	public static Boolean ShouldSerializeParentContainer() => false;
}
