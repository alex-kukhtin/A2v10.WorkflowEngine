// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System.Dynamic;

namespace A2v10.Workflow.Bpmn;
public class CallActivity : BpmnTask
{
	public String? CalledElement { get; set; }

    // bpmn:script
    public String? Script => ExtensionElements<A2v10.Workflow.Script>()?.FirstOrDefault()?.Text;
    public String? Parameters => ExtensionElements<A2v10.Workflow.Parameters>()?.FirstOrDefault()?.Text;

    protected override bool CanInduceIdle => true;

    public override async ValueTask ExecuteBody(IExecutionContext context)
    {
        if (CalledElement == null)
            throw new WorkflowException("Called element is null");
        var prms = context.Evaluate<ExpandoObject>(Id, nameof(Parameters));
        var result = await context.Call(CalledElement, prms);
        if (result.ExecutionStatus == WorkflowExecutionStatus.Complete)
        {
            await context.HandleEndEvent(result.State?.Get<ExpandoObject>("EndEvent"));
            if (IsComplete)
                return;
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
        if (!String.IsNullOrEmpty(Script))
            context.ExecuteResult(Id, nameof(Script), result);
        await CompleteBody(context);
    }

    public override void BuildScriptBody(IScriptBuilder builder)
    {
        builder.BuildExecuteResult(nameof(Script), Script);
        builder.BuildEvaluate(nameof(Parameters), Parameters);
    }
}
