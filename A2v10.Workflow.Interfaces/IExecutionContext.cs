// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.Workflow.Interfaces
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;
	using ResumeAction = Func<IExecutionContext, String, Object, ValueTask>;
	using EventAction = Func<IExecutionContext, IWorkflowEvent, Object, ValueTask>;

	public interface IExecutionContext
	{
		void Schedule(IActivity activity, ExecutingAction onComplete, IToken token);

		void SetBookmark(String bookmark, IActivity activity, ResumeAction onComplete);
		void RemoveBookmark(String bookmark);

		void AddEvent(IWorkflowEvent wfEvent, IActivity activity, EventAction onComplete);
		void RemoveEvent(String eventKey);

		T Evaluate<T>(String refer, String name);
		void Execute(String refer, String name);
		void ExecuteResult(String refer, String name, Object result);
	}
}
