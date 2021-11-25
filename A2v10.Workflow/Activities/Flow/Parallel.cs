// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	public enum CompletionCondition
	{
		Any,
		All
	}

	public class Parallel : Activity, IScoped
	{

		public List<IVariable>? Variables { get; set; }
		public String? GlobalScript { get; set; }

		public List<IActivity>? Branches { get; set; }

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

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
		{
			if (Branches == null || Branches.Count == 0)
			{
				return ValueTask.CompletedTask;
			}
			foreach (var br in Branches)
				context.Schedule(br, token);
			return ValueTask.CompletedTask;
		}

		public override void TryComplete(IExecutionContext context, IActivity activity)
		{
			// TODO: use CompletionCondition
			base.TryComplete(context, activity);
		}
	}
}
