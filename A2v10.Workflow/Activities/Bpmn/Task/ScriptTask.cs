// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.


namespace A2v10.Workflow.Bpmn;
public class ScriptTask : BpmnTask
{
	// bpmn:script
	public String? Script => Children?.OfType<A2v10.Workflow.Bpmn.Script>().FirstOrDefault()?.Text;

	public override ValueTask ExecuteBody(IExecutionContext context)
	{
		IsComplete = true;
		if (!String.IsNullOrEmpty(Script))
			context.Execute(Id, nameof(Script));
		return CompleteBody(context);
	}

	public override void BuildScriptBody(IScriptBuilder builder)
	{
		builder.BuildExecute(nameof(Script), Script);
	}
}

