// Copyright © 2022 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Engine;

public class WorkflowEngineOptions
{
    public IEnumerable<NativeType>? NativeTypes { get; set; }
}

