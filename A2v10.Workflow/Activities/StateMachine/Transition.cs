// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	public class Transition : Activity, IScriptable
	{
		public String Condition { get; set; }

		public IActivity Trigger { get; set; }
		public IActivity Action { get; set; }

		public String Destination { get; set; }

		internal String NextState { get; set; }

		private State ParentState => Parent as State;

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token)
		{
			NextState = null;
			if (Trigger != null)
				context.Schedule(Trigger, token);
			else
				ContinueExecute(context);
			return ValueTask.CompletedTask;
		}


		public override void TryComplete(IExecutionContext context, IActivity activity)
		{
			if (activity == Trigger)
			{
				ContinueExecute(context);
			} 
			else if (activity == Action)
			{
				ParentState.TransitionComplete(context, this);
			}
		}

		void ContinueExecute(IExecutionContext context)
		{
			var cond = context.Evaluate<Boolean>(Id, nameof(Condition));
			if (cond)
			{
				NextState = Destination;
				if (Action != null)
				{
					context.Schedule(Action, null);
					return;
				}
			}
			ParentState.TransitionComplete(context, this);
		}

		public override IEnumerable<IActivity> EnumChildren()
		{
			if (Trigger != null)
				yield return Trigger;
			if (Action != null)
				yield return Action;
		}

		#region IScriptable
		public void BuildScript(IScriptBuilder builder)
		{
			builder.BuildEvaluate(nameof(Condition), Condition);
		}
		#endregion
	}
}
