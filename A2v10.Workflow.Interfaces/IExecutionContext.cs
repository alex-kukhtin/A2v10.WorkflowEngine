// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.Workflow.Interfaces;

using EventAction = Func<IExecutionContext, IWorkflowEvent, Object?, ValueTask>;
using ResumeAction = Func<IExecutionContext, String, Object?, ValueTask>;

public interface IExecutionContext
{
    void Schedule(IActivity activity, IToken? token);

    void SetBookmark(String bookmark, IActivity activity, ResumeAction onComplete);
    void RemoveBookmark(String bookmark);
    void RemoveBookmarks(String activity);

    void SetInbox(Guid id, ExpandoObject inbox, IActivity activity, String bookmark);
    void RemoveInbox(Guid? id, String? Answer);

    void AddEvent(IWorkflowEvent wfEvent, IActivity activity, EventAction onComplete);
    void RemoveEvent(String eventKey);
    void AddTrack(ExpandoObject? track, IActivity activity);

    T? Evaluate<T>(String refer, String name);
    void Execute(String refer, String name);
    void ExecuteResult(String refer, String name, Object? result);
    void SetLastResult(Object? result); 
    void SetVariable(String refer, String name, Object? value);

    ValueTask<IInstance> Call(String activity, String? correlationId, ExpandoObject? prms, IToken? token = null);
    ValueTask CancelChildren(String workflow);
    void CancelActivity(IActivity activity);  
    ValueTask HandleEvent(IWorkflowEvent evt);
    void ProcessEndEvent(IWorkflowEvent evt);
    ValueTask HandleEndEvent(ExpandoObject? evt);

    ValueTask<DateTime> Now();
    void AppendSignal(List<ExpandoObject>? signals);
    void MergeSignal(IInstance instance);
}

