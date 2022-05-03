// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Bpmn
{
    public class TimeCycle : TimeBase
    {
        public override Boolean CanRepeat => true;
        public override DateTime NextTriggerTime(Object? span)
        {
            if (span is String strSpan && TimeSpan.TryParse(strSpan, out TimeSpan timeSpan))
                return DateTime.UtcNow + timeSpan;
            throw new WorkflowException($"TimeCycle.NextTriggerTime can't convert from {span}");
        }
    }
}
