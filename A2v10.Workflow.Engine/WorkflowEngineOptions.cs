// Copyright © 2022-2023 Oleksandr Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using System.Collections.Generic;

namespace A2v10.Workflow.Engine;

public class WorkflowEngineOptions
{
    public IEnumerable<NativeType>? NativeTypes { get; set; }
}

