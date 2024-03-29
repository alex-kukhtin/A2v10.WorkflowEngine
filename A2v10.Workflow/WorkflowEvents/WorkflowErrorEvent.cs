﻿// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;

namespace A2v10.Workflow;
public class WorkflowErrorEvent : IWorkflowEvent
{
    public String Key { get; }
    public String Ref { get; }

    public EventKind Kind => EventKind.Error;

    public WorkflowErrorEvent(String key, String refer)
    {
        Key = key;
        Ref = refer;
    }

    public WorkflowErrorEvent(String key, ExpandoObject exp)
    {
        Key = key;
        Ref = exp.GetNotNull<String>("Text");
    }

    // State
    public ExpandoObject ToExpando()
    {
        return new ExpandoObject()
        {
            { "Kind", "Error"},
            { "Text", Ref}
        };
    }

    // Instance Store
    public ExpandoObject ToStore()
    {
        return new ExpandoObject()
        {
            { "Event", Key},
            { "Kind", "E" } /*E(rror)*/,
            { "Text", Ref }
        };
    }

    public override String ToString()
    {
        return $"{{key: '{Key}', kind: 'Error', ref:'{Ref}'}}";
    }

}

