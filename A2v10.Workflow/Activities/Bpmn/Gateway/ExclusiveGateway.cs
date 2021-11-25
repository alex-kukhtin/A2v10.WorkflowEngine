// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn;
public class ExclusiveGateway : Gateway
{
	public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
	{
		SequenceFlow? flowToExecute = FindFlowToExecute(context);
		if (flowToExecute != null)
			context.Schedule(flowToExecute, token);
		return ValueTask.CompletedTask;
	}

	SequenceFlow? FindFlowToExecute(IExecutionContext context)
	{
		if (!HasOutgoing)
			return null;
		if (Outgoing!.Count() == 1)
		{
			// join flows
			return ParentContainer.FindElement<SequenceFlow>(Outgoing!.ElementAt(0).Text);
		}

		foreach (var og in Outgoing!)
		{
			var flow = ParentContainer.FindElement<SequenceFlow>(og.Text);
			if (flow != null)
			{
				if (flow.Evaluate(context))
					return flow;
			}
		}
		if (!String.IsNullOrEmpty(Default))
			return ParentContainer.FindElement<SequenceFlow>(Default);
		return null;
	}
}

