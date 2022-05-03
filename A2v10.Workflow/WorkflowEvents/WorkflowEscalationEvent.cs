// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Dynamic;

namespace A2v10.Workflow;
public class WorkflowEscalationEvent : IWorkflowEvent
{
    public String Key { get; }
    public String Ref { get; }

    public EventKind Kind => EventKind.Escalation;

    public WorkflowEscalationEvent(String key, String refer)
    {
        Key = key;
        Ref = refer;
    }

    public WorkflowEscalationEvent(String key, ExpandoObject exp)
    {
        Key = key;
        Ref = exp.GetNotNull<String>("Text");
    }

    // State
    public ExpandoObject ToExpando()
    {
        return new ExpandoObject()
        {
            { "Kind", "Escalation"},
            { "Text", Ref}
        };
    }

    // Instance Store
    public ExpandoObject ToStore()
    {
        return new ExpandoObject()
        {
            { "Event", Key},
            { "Kind", "S" } /*e(S)calation)*/,
            { "Text", Ref }
        };
    }

    public override String ToString()
    {
        return $"{{key: '{Key}', kind: 'Escalation', ref:'{Ref}'}}";
    }

}

