// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Linq;
using System.Threading.Tasks;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	public class BpmnTask : FlowElement, IStorable, ICanComplete
	{

		protected ExecutingAction _onComplete;
		protected IToken _token;

		public Boolean IsComplete { get; protected set; }

		protected virtual Boolean CanInduceIdle => false;

		#region IStorable 
		const String ON_COMPLETE = "OnComplete";
		const String TOKEN = "Token";

		public virtual void Store(IActivityStorage storage)
		{
			if (!CanInduceIdle) return;
			storage.SetCallback(ON_COMPLETE, _onComplete);
			storage.SetToken(TOKEN, _token);
		}

		public virtual void Restore(IActivityStorage storage)
		{
			if (!CanInduceIdle) return;
			_onComplete = storage.GetCallback(ON_COMPLETE);
			_token = storage.GetToken(TOKEN);
		}
		#endregion



		public override async ValueTask ExecuteAsync(IExecutionContext context, IToken token, ExecutingAction onComplete)
		{
			_onComplete = onComplete;
			_token = token;
			// boundary events
			foreach (var ev in Parent.FindAll<BoundaryEvent>(ev => ev.AttachedToRef == Id))
				await ev.ExecuteAsync(context, Parent.NewToken(), EventComplete);

			// loop here
			await ExecuteBody(context);
		}

		public override void Cancel(IExecutionContext context)
		{
			CompleteTask(context);
			context.RemoveBookmark(Id);
			base.Cancel(context);
		}


		protected virtual ValueTask CompleteBody(IExecutionContext context)
		{
			if (Outgoing == null)
			{
				if (_onComplete != null)
					return _onComplete(context, this);
				return ValueTask.CompletedTask;
			}

			if (Outgoing.Count() == 1)
			{
				// simple outgouning - same token
				var targetFlow = Parent.FindElement<SequenceFlow>(Outgoing.First().Text);
				context.Schedule(targetFlow, _onComplete, _token);
				_token = null;
			}
			else
			{
				// same as task + parallelGateway
				Parent.KillToken(_token);
				_token = null;
				foreach (var flowId in Outgoing)
				{
					var targetFlow = Parent.FindElement<SequenceFlow>(flowId.Text);
					context.Schedule(targetFlow, _onComplete, Parent.NewToken());
				}
			}
			if (_onComplete != null)
				return _onComplete(context, this);
			return ValueTask.CompletedTask;
		}

		public virtual ValueTask ExecuteBody(IExecutionContext context)
		{
			return CompleteBody(context);
		}

		[StoreName("OnEventComplete")]
		protected virtual ValueTask EventComplete(IExecutionContext context, IActivity activity)
		{
			return ValueTask.CompletedTask;
		}

		protected virtual void CompleteTask(IExecutionContext context)
		{
			IsComplete = true;
			foreach (var ev in Parent.FindAll<BoundaryEvent>(ev => ev.AttachedToRef == Id))
				context.RemoveEvent(ev.Id);
		}
	}
}
