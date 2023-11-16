// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using A2v10.System.Xaml;

namespace A2v10.Workflow.Bpmn;

[ContentProperty("Text")]
public abstract class FlowDirection : BaseElement
{
    private String? _text;
    public String Text
    {
        get => _text ?? throw new InvalidOperationException("FlowDirection.Text is null");
        init => _text = value;
    }
    public abstract Boolean IsIncoming { get; }
}

public class Incoming : FlowDirection
{
    public override Boolean IsIncoming => true;
}

public class Outgoing : FlowDirection
{
    public override Boolean IsIncoming => false;
}

