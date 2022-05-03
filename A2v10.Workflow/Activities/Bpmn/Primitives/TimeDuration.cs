// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Bpmn;

public class TimeDuration : TimeBase
{
    public override Boolean CanRepeat => false;

    public override DateTime NextTriggerTime(Object? span)
    {
        if (span is String strSpan && TimeSpan.TryParse(strSpan, out TimeSpan timeSpan))
            return DateTime.UtcNow + timeSpan;
        throw new WorkflowException($"TimeDuration.NextTriggerTime can't convert from {span}");
    }
}

