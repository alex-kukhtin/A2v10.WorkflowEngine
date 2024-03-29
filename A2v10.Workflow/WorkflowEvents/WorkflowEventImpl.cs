﻿// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;


namespace A2v10.Workflow;
public static class WorkflowEventImpl
{
    public static IWorkflowEvent FromExpando(String key, ExpandoObject exp)
    {
        var kind = exp.Get<String>("Kind");
        return kind switch
        {
            "Timer" => new WorkflowTimerEvent(key, exp),
            "Message" => new WorkflowMessageEvent(key, exp),
            "Error" => new WorkflowErrorEvent(key, exp),
            "Escalation" => new WorkflowEscalationEvent(key, exp),
            _ => throw new WorkflowException($"Invalid event kind ({kind})"),
        };
    }
}

