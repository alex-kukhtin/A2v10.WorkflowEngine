﻿// Copyright © 2021 Oleksandr Kukhtin. All rights reserved.

using A2v10.System.Xaml;


namespace A2v10.Workflow.Bpmn;

[ContentProperty("Expression")]
public class LoopCondition : BaseElement
{
    public String? Type { get; init; }
    public String? Expression { get; init; }
}
