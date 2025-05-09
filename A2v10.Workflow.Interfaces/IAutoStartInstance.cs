﻿// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public interface IAutoStartInstance
{
    Int64 Id { get; }
    String? WorkflowId { get; }
    Int32 Version { get; }
    String? CorrelationId { get; }
    ExpandoObject? Params { get; }
    Guid? InstanceId { get; }
}

