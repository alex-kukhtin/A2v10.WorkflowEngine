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
	public class Process : ProcessBase
	{
		public Boolean IsExecutable { get; init; }
		public Boolean IsClosed { get; init; }

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token, ExecutingAction onComplete)
		{
			_onComplete = onComplete;
			_token = token;
			if (!IsExecutable || Children == null)
				return ValueTask.CompletedTask;
			var start = Elems<Event>().FirstOrDefault(ev => ev.IsStart);
			if (start == null)
				throw new WorkflowException($"Process (Id={Id}). Start event not found");
			context.Schedule(start, OnElemComplete, token);
			return ValueTask.CompletedTask;
		}

		public override void TryComplete(IExecutionContext context)
		{
			if (TokensCount > 0)
				return;
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
				return ProcessComplete(context);
			}
			return ValueTask.CompletedTask;
		}

		ValueTask ProcessComplete(IExecutionContext context)
		{
			if (_onComplete != null)
				return _onComplete(context, this);
			return ValueTask.CompletedTask;
		}

		public override void OnEndInit()
		{
			if (Children == null)
				return;
			foreach (var e in Activities)
			{
				e.SetParent(this);
				if (e is IContainer subProcess)
					subProcess.OnEndInit();
			}
		}
	}
}
