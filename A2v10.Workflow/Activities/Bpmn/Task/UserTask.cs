// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.


using System.Dynamic;

namespace A2v10.Workflow.Bpmn;
public class UserTask : BpmnTask
{
    // wf:Script here
    public String? Script => ExtensionElements<A2v10.Workflow.Script>()?.FirstOrDefault()?.Text;
    public String? Inbox => ExtensionElements<A2v10.Workflow.Inbox>()?.FirstOrDefault()?.Text;

    protected override bool CanInduceIdle => true;

    private Guid? _inboxId;

    public override ValueTask ExecuteBody(IExecutionContext context)
    {
        context.SetBookmark(Id, this, OnUserTaskComplete);
        var inbox = context.Evaluate<ExpandoObject>(Id, nameof(Inbox));
        if (inbox != null)
        {
            _inboxId = Guid.NewGuid();
            context.SetInbox(_inboxId.Value, inbox, this);
        }
        return ValueTask.CompletedTask;
    }

    [StoreName("OnUserTaskComplete")]
    ValueTask OnUserTaskComplete(IExecutionContext context, String bookmark, Object? result)
    {
        CompleteTask(context);
        context.RemoveBookmark(bookmark);
        context.RemoveInbox(_inboxId);
        if (!String.IsNullOrEmpty(Script))
            context.ExecuteResult(Id, nameof(Script), result);
        return CompleteBody(context);
    }

    public override void Cancel(IExecutionContext context)
    {
        base.Cancel(context);
        context.RemoveInbox(_inboxId);
    }

    public override void BuildScriptBody(IScriptBuilder builder)
    {
        builder.BuildExecuteResult(nameof(Script), Script);
        builder.BuildEvaluate(nameof(Inbox), Inbox);
    }

    #region IStorable 

    public override void Store(IActivityStorage storage)
    {
        base.Store(storage);
        if (_inboxId != null)
            storage.Set<Guid>(nameof(Inbox), _inboxId.Value);
    }

    public override void Restore(IActivityStorage storage)
    {
        base.Restore(storage);
        var strInbox = storage.Get<String>(nameof(Inbox));
        if (strInbox != null)
            _inboxId = Guid.Parse(strInbox);
    }
    #endregion
}

