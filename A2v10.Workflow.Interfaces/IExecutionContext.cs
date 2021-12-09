// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.


namespace A2v10.Workflow.Interfaces;

using ResumeAction = Func<IExecutionContext, String, Object?, ValueTask>;
using EventAction = Func<IExecutionContext, IWorkflowEvent, Object?, ValueTask>;

public interface IExecutionContext
{
	void Schedule(IActivity activity, IToken? token);

	void SetBookmark(String bookmark, IActivity activity, ResumeAction onComplete);
	void RemoveBookmark(String bookmark);

	void AddEvent(IWorkflowEvent wfEvent, IActivity activity, EventAction onComplete);
	void RemoveEvent(String eventKey);

	T? Evaluate<T>(String refer, String name);
	void Execute(String refer, String name);
	void ExecuteResult(String refer, String name, Object? result);

	ValueTask<IInstance> Call(String activity, ExpandoObject? prms);

	ValueTask HandleEvent(IWorkflowEvent evt);
}

