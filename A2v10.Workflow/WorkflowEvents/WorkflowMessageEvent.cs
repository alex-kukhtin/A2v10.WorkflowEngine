// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Dynamic;

namespace A2v10.Workflow;
public class WorkflowMessageEvent : IWorkflowEvent
{
    public String Key { get; }
    public String Ref { get; }

    public EventKind Kind => EventKind.Message;

    public WorkflowMessageEvent(String key, String refer)
    {
        Key = key;
        Ref = refer;
    }

    public WorkflowMessageEvent(String key, ExpandoObject exp)
    {
        Key = key;
        Ref = exp.GetNotNull<String>("Text");
    }

    // State
    public ExpandoObject ToExpando()
    {
        return new ExpandoObject()
        {
            { "Kind", "Message"},
            { "Text", Ref}
        };
    }

    // Instance Store
    public ExpandoObject ToStore()
    {
        return new ExpandoObject()
        {
            { "Event", Key},
            { "Kind", "M" } /*M(essage)*/,
            { "Text", Ref }
        };
    }

    public override String ToString()
    {
        return $"{{key: '{Key}', kind: 'Message', ref:'{Ref}'}}";
    }

}

