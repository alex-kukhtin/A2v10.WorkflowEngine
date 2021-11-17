// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	public class State : StateBase, IStorable
	{
		public IActivity Entry { get; set; }
		public IActivity Exit { get; set; }

		public List<Transition> Transitions { get; set; }

		IToken _token;

		#region IStorable
		const String TOKEN = "Token";

		public void Store(IActivityStorage storage)
		{
			storage.SetToken(TOKEN, _token);
		}

		public void Restore(IActivityStorage storage)
		{
			_token = storage.GetToken(TOKEN);
		}
		#endregion

		public override IEnumerable<IActivity> EnumChildren()
		{
			if (Entry != null)
				yield return Entry;
			if (Transitions != null)
				foreach (var tr in Transitions)
					yield return tr;
			if (Exit != null)
				yield return Exit;
		}

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token)
		{
			_token = token;
			NextState = null;
			if (Entry != null)
				context.Schedule(Entry, token);
			else if (ScheduleTransitions(context))
				return ValueTask.CompletedTask;
			else
				ScheduleExit(context);
			return ValueTask.CompletedTask;
		}

		public override void TryComplete(IExecutionContext context, IActivity activity)
		{
			if (activity == Entry) {
				if (ScheduleTransitions(context))
					return;
			}
			Parent.TryComplete(context, this);
		}

		// Schedule all transitions.
		Boolean ScheduleTransitions(IExecutionContext context)
		{
			if (Transitions == null || Transitions.Count == 0)
				return false;
			foreach (var tr in Transitions)
				context.Schedule(tr, _token);
			return true;
		}

		private void ScheduleExit(IExecutionContext context)
		{
			if (Exit != null)
				context.Schedule(Exit, _token);
			else
				Parent.TryComplete(context, this);
		}

		public void TransitionComplete(IExecutionContext context, Transition transition)
		{
			if (transition.NextState != null)
			{
				// Transition completed. Returning with new state.
				NextState = transition.NextState;
				ScheduleExit(context);
			}
			else
				Parent.TryComplete(context, this);
		}

		public override void OnEndInit(IActivity parent)
		{
			base.OnEndInit(parent);
			Entry?.OnEndInit(this);
			Exit?.OnEndInit(this);
			if (Transitions != null)
				foreach (var tr in Transitions)
					tr.OnEndInit(this);
		}
	}
}