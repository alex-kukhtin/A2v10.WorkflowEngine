// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Linq;
using System.Threading.Tasks;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	public class BpmnTask : FlowElement, IStorable, ICanComplete, IScriptable, ILoopable
	{

		protected ExecutingAction _onComplete;
		protected IToken _token;
		protected Int32 _loopCounter;

		public Boolean IsComplete { get; protected set; }

		protected virtual Boolean CanInduceIdle => false;

		#region IStorable 
		const String ON_COMPLETE = "OnComplete";
		const String TOKEN = "Token";
		const String LOOP_COUNTER = "LoopCounter";

		public virtual void Store(IActivityStorage storage)
		{
			if (!CanInduceIdle) return;
			storage.SetCallback(ON_COMPLETE, _onComplete);
			storage.SetToken(TOKEN, _token);
			if (HasLoop)
				storage.Set<Int32>(LOOP_COUNTER, _loopCounter);
		}

		public virtual void Restore(IActivityStorage storage)
		{
			if (!CanInduceIdle) return;
			_onComplete = storage.GetCallback(ON_COMPLETE);
			_token = storage.GetToken(TOKEN);
			if (HasLoop)
				_loopCounter = storage.Get<Int32>(LOOP_COUNTER);
		}
		#endregion


		public override async ValueTask ExecuteAsync(IExecutionContext context, IToken token, ExecutingAction onComplete)
		{
			_onComplete = onComplete;
			_token = token;

			if (HasLoop && TestBefore && !CanCountinue(context))
			{
				await DoCompleteBody(context);
				return;
			}

			if (_loopCounter == 0)
			{
				// boundary events
				foreach (var ev in Parent.FindAll<BoundaryEvent>(ev => ev.AttachedToRef == Id))
					await ev.ExecuteAsync(context, Parent.NewToken(), EventComplete);
			}
			_loopCounter += 1;

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
			if (HasLoop && (TestBefore || CanCountinue(context)))
			{
				IsComplete = false;
				context.Schedule(this, _onComplete, _token);
				return ValueTask.CompletedTask;
			}
			return DoCompleteBody(context);
		}

		ValueTask DoCompleteBody(IExecutionContext context) 
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
			CompleteTask(context);
			if (_onComplete != null)
				return _onComplete(context, this);
			return ValueTask.CompletedTask;
		}

		public virtual ValueTask ExecuteBody(IExecutionContext context)
		{
			return CompleteBody(context);
		}

		public virtual void BuildScriptBody(IScriptBuilder builder)
		{
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

		#region IScriptable
		public void BuildScript(IScriptBuilder builder)
		{
			if (HasLoop)
			{
				var expr = LoopCharacteristics?.LoopCondition;
				if (String.IsNullOrEmpty(expr))
					expr = "true";
				builder.BuildEvaluate(LoopConditionEval, expr);
			}
			BuildScriptBody(builder);
		}
		#endregion

		public String LoopConditionEval => $"{Id}_Loop";
		public StandardLoopCharacteristics LoopCharacteristics => Children?.OfType<StandardLoopCharacteristics>().FirstOrDefault();
		public Boolean TestBefore => LoopCharacteristics?.TestBefore ?? false;
		public Int32 LoopMaximum => LoopCharacteristics?.LoopMaximum ?? 0;

		#region ILoopable

		public Boolean HasLoop => Children != null && Children.OfType<StandardLoopCharacteristics>().Any();

		public Boolean CanCountinue(IExecutionContext context)
		{
			if (!HasLoop) 
				return false;
			var max = LoopMaximum;
			if (max != 0 && _loopCounter >= max)
				return false;
			return context.Evaluate<Boolean>(Id, LoopConditionEval);
		}
	}
	#endregion
}

