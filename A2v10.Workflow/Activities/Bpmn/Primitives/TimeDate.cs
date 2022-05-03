// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Bpmn
{
    public class TimeDate : TimeBase
    {
        public override Boolean CanRepeat => false;

        public override DateTime NextTriggerTime(Object? arg)
        {
            if (arg is DateTime dateTime)
                return dateTime;
            throw new WorkflowException($"TimeDate.NextTriggerTime can't convert from {arg}");
        }
    }
}
