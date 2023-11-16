// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using A2v10.System.Xaml;
using A2v10.Workflow.Bpmn;

namespace A2v10.Workflow;

[ContentProperty("Text")]
public class GlobalScript : BaseElement
{
    public String? Text { get; init; }
}
