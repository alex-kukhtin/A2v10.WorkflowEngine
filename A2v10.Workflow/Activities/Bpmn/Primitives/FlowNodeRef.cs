// Copyright © 2022 Oleksandr Kukhtin. All rights reserved.

using A2v10.System.Xaml;


namespace A2v10.Workflow.Bpmn;

[ContentProperty("Text")]
public class FlowNodeRef : BaseElement
{
    public String? Text { get; init; }
}

