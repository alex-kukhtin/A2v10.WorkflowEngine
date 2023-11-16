// Copyright © 2021 Oleksandr Kukhtin. All rights reserved.

using System;
using System.Dynamic;

namespace A2v10.Workflow.WebHost.Models;
public class CreateRequest
{
    public String? Workflow { get; set; }
    public Int32 Version { get; set; }

    public String? CorrelationId { get; set; }
}

public class CreateResponse
{
    public Guid InstanceId { get; init; }
}

public class RunRequest
{
    public Guid InstanceId { get; init; }
    public ExpandoObject? Parameters { get; init; }
}

public record RunResponse
{
    public RunResponse(String status)
    {
        ExecutionStatus = status;
    }
    public String ExecutionStatus { get; init; }
    public ExpandoObject? Result { get; init; }
}

