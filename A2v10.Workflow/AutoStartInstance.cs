// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Dynamic;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow;
public class AutoStartInstance : IAutoStartInstance
{
    public Int64 Id { get; init; }
    public String? WorkflowId { get; init; }

    public Int32 Version { get; init; }

    public ExpandoObject? Params { get; init; }
}

