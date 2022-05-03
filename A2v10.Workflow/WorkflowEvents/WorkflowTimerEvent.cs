// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Dynamic;

namespace A2v10.Workflow;
public class WorkflowTimerEvent : IWorkflowEvent
{
    public String Key { get; }

    public String Ref { get; } = String.Empty;

    public DateTime TriggerTime { get; }

    public EventKind Kind => EventKind.Timer;

    public WorkflowTimerEvent(String key, DateTime triggerTime)
    {
        Key = key;
        TriggerTime = triggerTime;
    }

    public WorkflowTimerEvent(String key, ExpandoObject exp)
    {
        Key = key;
        TriggerTime = exp.Get<DateTime>("Pending");
    }

    public ExpandoObject ToExpando()
    {
        return new ExpandoObject()
        {
            { "Kind", "Timer"},
            { "Pending", TriggerTime.ToString("O") }
        };
    }

    public ExpandoObject ToStore()
    {
        return new ExpandoObject()
        {
            { "Event", Key},
            { "Kind", "T"}, /*T(imer)*/
			{ "Pending", TriggerTime }
        };
    }

    public override String ToString()
    {
        return $"{{key: '{Key}', kind: 'Timer', pending:'{TriggerTime:O}'}}";
    }

}

