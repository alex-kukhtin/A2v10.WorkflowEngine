// Copyright © 2020-2022 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Bpmn;
public class TimerEventDefinition : EventDefinition, IScriptable
{
    TimeBase? TimeBase => Children?.OfType<TimeBase>().FirstOrDefault();

    public async override ValueTask<IWorkflowEvent> CreateEvent(String id, IExecutionContext context)
    {
        return new WorkflowTimerEvent(id, await NextTriggerTime(context));
    }

    private ValueTask<DateTime> NextTriggerTime(IExecutionContext context)
    {
        if (TimeBase == null)
            throw new WorkflowException($"TimerEventDefinition. There is no trigger time for '{Id}'");
        Object? timeSpan = TimeBase.Expression;
        if (TimeBase.Expression.IsVariable())
            timeSpan = context.Evaluate<Object>(Id, TimeBaseEvaluate);
        return TimeBase.NextTriggerTime(context, timeSpan);
    }

    public override Boolean CanRepeat => TimeBase is { CanRepeat: true };

    String TimeBaseEvaluate => $"{Id}_TimeBase";

    public void BuildScript(IScriptBuilder builder)
    {
        if (TimeBase == null || !TimeBase.Expression.IsVariable())
            return;
        builder.BuildEvaluate(TimeBaseEvaluate, TimeBase.Expression.Variable());
    }
}

