// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using A2v10.System.Xaml;

namespace A2v10.Workflow.Bpmn;

[ContentProperty("Expression")]
public abstract class TimeBase : BaseElement
{
    public String Type { get; init; } = String.Empty;
    public String Expression { get; init; } = String.Empty;

    public abstract Boolean CanRepeat { get; }
    public abstract ValueTask<DateTime> NextTriggerTime(IExecutionContext context, Object? arg);
}

