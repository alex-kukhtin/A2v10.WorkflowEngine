// Copyright © 2020-2022 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public class AutoStartInstance : IAutoStartInstance
{
    public Int64 Id { get; init; }
    public String? WorkflowId { get; init; }

    public Int32 Version { get; init; }

    public ExpandoObject? Params { get; init; }
}
