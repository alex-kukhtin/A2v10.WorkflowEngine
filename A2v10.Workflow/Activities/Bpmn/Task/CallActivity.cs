﻿// Copyright © 2021-2025 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;

namespace A2v10.Workflow.Bpmn;
public class CallActivity : BpmnTask
{
    public String? CalledElement { get; set; }
    public String? CorrelationId { get; set; } // Expression!
    // bpmn:script
    public String? Script => ExtensionElements<A2v10.Workflow.Script>()?.FirstOrDefault()?.Text;
    public String? Parameters => ExtensionElements<A2v10.Workflow.Parameters>()?.FirstOrDefault()?.Text;

    protected override bool CanInduceIdle => true;

    String CalledElemEvaluate => $"{Id}_CalledElement";

    public override async ValueTask ExecuteBody(IExecutionContext context)
    {
        if (CalledElement == null)
            throw new WorkflowException("Called element is null");
        var prms = context.Evaluate<ExpandoObject>(Id, nameof(Parameters));
        var correlationId = CorrelationId != null 
             ? context.Evaluate<String>(Id, nameof(CorrelationId))
             : null;
        var calledElem = CalledElement;
        if (calledElem.IsVariable())
            calledElem = context.Evaluate<String>(Id, CalledElemEvaluate)
                ?? throw new WorkflowException("Expression: Called element is null");

        var result = await context.Call(calledElem, correlationId, prms);
        if (result.ExecutionStatus == WorkflowExecutionStatus.Complete)
        {
            await context.HandleEndEvent(result.State?.Get<ExpandoObject>("EndEvent"));
            if (IsComplete)
                return;
            context.SetLastResult(result.Result);
            context.ExecuteResult(Id, nameof(Script), result.Result);
            await CompleteBody(context);
        }
        else if (result.ExecutionStatus == WorkflowExecutionStatus.Idle)
            context.SetBookmark($"{result.Workflow.Identity.Id}:{result.Id}", this, OnActivityComplete);
    }

    [StoreName("OnActivityComplete")]
    async ValueTask OnActivityComplete(IExecutionContext context, String bookmark, Object? result)
    {
        if (result is ExpandoObject resexp)
        {
            var ee = resexp.Get<ExpandoObject>("EndEvent");
            if (ee != null)
                await context.HandleEndEvent(ee);
        }
        context.RemoveBookmark(bookmark);
        if (IsComplete)
            return;
        CompleteTask(context);
        context.SetLastResult(result);
        if (!String.IsNullOrEmpty(Script))
            context.ExecuteResult(Id, nameof(Script), result);
        await CompleteBody(context);
    }

    public override void BuildScriptBody(IScriptBuilder builder)
    {
        builder.BuildExecuteResult(nameof(Script), Script);
        builder.BuildEvaluate(nameof(Parameters), Parameters);
        builder.BuildEvaluate(nameof(CorrelationId), CorrelationId);
        if (CalledElement.IsVariable())
            builder.BuildEvaluate(CalledElemEvaluate, CalledElement.Variable());
    }
}
