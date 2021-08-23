// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	public class Code : Activity, IScriptable
	{
		public String Script { get; set; }

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token, ExecutingAction onComplete)
		{
			context.Execute(Id, nameof(Script));
			if (onComplete != null)
				return onComplete(context, this);
			return ValueTask.CompletedTask;
		}

		#region IScriptable
		public void BuildScript(IScriptBuilder builder)
		{
			builder.BuildExecute(nameof(Script), Script);
		}
		#endregion
	}
}
