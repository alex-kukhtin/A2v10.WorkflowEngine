// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.


namespace A2v10.Workflow.Interfaces;


public enum EventKind
{
    Message,
    Error,
    Escalation,
    Timer
}

public interface IWorkflowEvent
{
    String Key { get; }
    String Ref { get; }
    EventKind Kind { get; }
    ExpandoObject ToExpando();
    ExpandoObject ToStore();
}

