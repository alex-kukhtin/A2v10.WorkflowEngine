
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
	public class SubProcess : ProcessBase, ILoopable
	{
		public Boolean IsClosed { get; init; }

		protected Int32 _loopCounter;

		const String LOOP_COUNTER = "LoopCounter";

		public override void Store(IActivityStorage storage)
		{
			base.Store(storage);
			if (HasLoop)
				storage.Set<Int32>(LOOP_COUNTER, _loopCounter);
		}

		public override void Restore(IActivityStorage storage)
		{
			base.Restore(storage);
			if (HasLoop)
				_loopCounter = storage.Get<Int32>(LOOP_COUNTER);
		}

		public override void BuildScript(IScriptBuilder builder)
		{
			base.BuildScript(builder);
			if (HasLoop)
			{
				var expr = LoopCharacteristics?.LoopCondition;
				if (String.IsNullOrEmpty(expr))
					expr = "true";
				builder.BuildEvaluate(LoopConditionEval, expr);
			}
		}

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token, ExecutingAction onComplete)
		{
			_onComplete = onComplete;
			_token = token;

			if (HasLoop && TestBefore && !CanCountinue(context))
			{
				return ProcessComplete(context);
			}
			_loopCounter += 1;
			return ExecuteBody(context);
		}

		public ValueTask ExecuteBody(IExecutionContext context)
		{ 
			if (Children == null)
				return ValueTask.CompletedTask;
			var start = Elems<Event>().FirstOrDefault(ev => ev.IsStart);
			if (start == null)
				throw new WorkflowException($"SubProcess (Id={Id}). Start event not found");
			context.Schedule(start, OnElemComplete, _token);
			return ValueTask.CompletedTask;
		}

		[StoreName("OnElemComplete")]
		ValueTask OnElemComplete(IExecutionContext context, IActivity activity)
		{
			if (activity is EndEvent)
			{
				if (TokensCount > 0)
				{
					// do nothing, there are tokens
					return ValueTask.CompletedTask;
				}
				if (HasLoop && (TestBefore || CanCountinue(context)))
				{
					context.Schedule(this, _onComplete, _token);
					return ValueTask.CompletedTask;
				}
				return ProcessComplete(context);
			}
			return ValueTask.CompletedTask;
		}

		ValueTask ProcessComplete(IExecutionContext context)
		{
			var count = Outgoing?.Count();
			if (count == 1)
			{
				// simple outgouning - same token
				var targetFlow = Parent.FindElement<SequenceFlow>(Outgoing.First().Text);
				context.Schedule(targetFlow, _onComplete, _token);
				_token = null;
			}
			else if (count > 1)
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

		public override void OnEndInit()
		{
			if (Children == null)
				return;
			foreach (var e in Activities)
				e.SetParent(this);
		}

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
		#endregion
	}
}
