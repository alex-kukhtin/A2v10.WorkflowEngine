// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using A2v10.System.Xaml;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	[ContentProperty("Children")]
	public abstract class Event : BpmnActivity, IScriptable, IStorable, ICanComplete
	{
		public virtual Boolean IsStart => false;

		public IEnumerable<Outgoing> Outgoing => Children?.OfType<Outgoing>();

		public EventDefinition EventDefinition => Children?.OfType<EventDefinition>().FirstOrDefault();

		public Boolean IsComplete { get; private set; }

		protected ExecutingAction _onComplete;
		protected IToken _token;

		#region IStorable
		const String ON_COMPLETE = "OnComplete";
		const String TOKEN = "Token";

		public void Store(IActivityStorage storage)
		{
			storage.SetCallback(ON_COMPLETE, _onComplete);
			storage.SetToken(TOKEN, _token);
		}

		public void Restore(IActivityStorage storage)
		{
			_onComplete = storage.GetCallback(ON_COMPLETE);
			_token = storage.GetToken(TOKEN);
		}
		#endregion


		// wf:Script here
		public String Script => ExtensionElements<A2v10.Workflow.Script>()?.FirstOrDefault()?.Text;

		#region IScriptable
		public void BuildScript(IScriptBuilder builder)
		{
			builder.BuildExecute(nameof(Script), Script);
		}
		#endregion

		protected void ScheduleOutgoing(IExecutionContext context)
		{
			if (Outgoing == null)
				return;
			if (Outgoing.Count() == 1)
			{
				var targetFlow = Parent.FindElement<SequenceFlow>(Outgoing.First().Text);
				context.Schedule(targetFlow, _onComplete, _token);
			}
			else
			{
				Parent.KillToken(_token);
				foreach (var o in Outgoing)
				{
					var targetFlow = Parent.FindElement<SequenceFlow>(o.Text);
					context.Schedule(targetFlow, _onComplete, Parent.NewToken());
				}
			}
		}

		public override void Cancel(IExecutionContext context)
		{
			SetComplete(context);
		}

		protected void SetComplete(IExecutionContext context)
		{
			IsComplete = true;
			context.RemoveEvent(Id);
		}
	}
}
