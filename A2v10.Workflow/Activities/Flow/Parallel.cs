// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	public enum CompletionCondition
	{
		Any,
		All
	}

	public class Parallel : ActivityWithComplete, IScoped
	{

		public List<IVariable> Variables { get; set; }
		public String GlobalScript { get; set; }

		public List<IActivity> Branches { get; set; }

		public CompletionCondition CompletionCondition { get; set; }

		#region IScriptable
		public virtual void BuildScript(IScriptBuilder builder)
		{
			builder.AddVariables(Variables);
		}
		#endregion

		public override IEnumerable<IActivity> EnumChildren()
		{
			if (Branches != null)
				foreach (var branch in Branches)
					yield return branch;
		}

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token, ExecutingAction onComplete)
		{
			_onComplete = onComplete;
			if (Branches == null || Branches.Count == 0)
			{
				if (onComplete != null)
					return onComplete(context, this);
				return ValueTask.CompletedTask;
			}
			foreach (var br in Branches)
				context.Schedule(br, OnBranchComplete, token);
			return ValueTask.CompletedTask;
		}

		[StoreName("OnBranchComplete")]
		ValueTask OnBranchComplete(IExecutionContext context, IActivity activity)
		{
			// TODO: CompletionCondition
			// TODO: cancel
			if (_onComplete != null)
				return _onComplete(context, this);
			return ValueTask.CompletedTask;
		}
	}
}
