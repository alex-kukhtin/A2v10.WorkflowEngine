// Copyright © 2020-2022 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Bpmn;

public class TimeDate : TimeBase
{
    public override Boolean CanRepeat => false;

    public override ValueTask<DateTime> NextTriggerTime(IExecutionContext context, Object? arg)
    {
        if (arg is DateTime dateTime)
            return ValueTask.FromResult<DateTime>(dateTime);
        throw new WorkflowException($"TimeDate.NextTriggerTime can't convert from {arg}");
    }
}
