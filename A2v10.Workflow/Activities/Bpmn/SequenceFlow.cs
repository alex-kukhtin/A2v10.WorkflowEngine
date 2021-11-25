// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.


using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn;
public class SequenceFlow : BpmnActivity, IScriptable
{
    private String? _sourceRef;
    private String? _targetRef;

    public String SourceRef { 
        get => _sourceRef ?? throw new InvalidProgramException("SourceRef is null"); 
        init => _sourceRef = value; 
    }
    public String TargetRef 
    {   get => _targetRef ?? throw new InvalidProgramException("TargetRef is null");
        init => _targetRef = value; 
    }

    public ConditionExpression? ConditionExpression => Children?.OfType<ConditionExpression>()?.FirstOrDefault();

    public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
    {
        var target = ParentContainer.FindElement<BpmnActivity>(TargetRef);
        context.Schedule(target, token);
        return ValueTask.CompletedTask;
    }

    #region IScriptable
    public void BuildScript(IScriptBuilder builder)
    {
        var expr = ConditionExpression;
        if (expr != null && !String.IsNullOrEmpty(expr.Expression))
            builder.BuildEvaluate(nameof(ConditionExpression), expr.Expression);
    }
    #endregion

    public Boolean Evaluate(IExecutionContext context)
    {
        var expr = ConditionExpression;
        if (String.IsNullOrEmpty(expr?.Expression))
            return false;
        return context.Evaluate<Boolean>(Id, nameof(ConditionExpression));
    }

}

