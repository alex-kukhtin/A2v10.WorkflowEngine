// Copyright © 2020-2022 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Bpmn;

public class TimeDuration : TimeBase
{
    public override Boolean CanRepeat => false;

    public override async ValueTask<DateTime> NextTriggerTime(IExecutionContext context, Object? span)
    {
        if (span is String strSpan && TimeSpan.TryParse(strSpan, out TimeSpan timeSpan))
            return await context.Now() + timeSpan;
        throw new WorkflowException($"TimeDuration.NextTriggerTime can't convert from {span}");
    }
}

