// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using System.Collections.Generic;
using A2v10.System.Xaml;
using A2v10.Workflow.Bpmn;

namespace A2v10.Workflow;

[ContentProperty("Items")]
public class Variables : BaseElement
{
    public List<Variable>? Items { get; init; }
}
