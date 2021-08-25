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


		protected IEnumerable<BpmnActivity> Activities => Elems<BpmnActivity>().ToList();

		public override IEnumerable<IActivity> EnumChildren()
		{
			if (Children != null)
				foreach (var elem in Activities)
					yield return elem;
		}

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

		[StoreName("OnElemComplete")]
		ValueTask OnElemComplete(IExecutionContext context, IActivity activity)
		{
			if (activity is EndEvent)
				return ProcessComplete(context);
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
				e.SetParent(this);
		}

		public T FindElement<T>(String id) where T : BpmnActivity
		{
			var elem = Activities.FirstOrDefault(e => e.Id == id);
			if (elem == null)
				throw new WorkflowException($"BPMN. Element (Id = {id}) not found");
			if (elem is T elemT)
				return elemT;
			throw new WorkflowException($"BPMN. Invalid type for element (Id = {id}). Expected: '{typeof(T).Name}', Actual: '{elem.GetType().Name}'");
		}

		public IEnumerable<T> FindAll<T>(Predicate<T> predicate) where T : BpmnActivity
		{
			var list = Activities.Where(elem => elem is T t && predicate(t));
			foreach (var el in list)
				yield return el as T;
		}
	}
}
