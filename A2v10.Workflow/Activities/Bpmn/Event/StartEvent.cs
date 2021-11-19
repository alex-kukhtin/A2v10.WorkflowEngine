// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	public class StartEvent : Event
	{
		public override Boolean IsStart => true;

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token)
		{
			if (!String.IsNullOrEmpty(Script))
				context.Execute(Id, nameof(Script));

			foreach (var flow in Outgoing)
			{
				var flowElem = ParentContainer.FindElement<SequenceFlow>(flow.Text);
				if (flowElem.SourceRef != Id)
					throw new WorkflowException($"BPMN. Invalid SequenceFlow (Id={Id}. SourceRef does not match");
				// generate new token for every outogoing flow!
				context.Schedule(flowElem, ParentContainer.NewToken());
			}
			return ValueTask.CompletedTask;
		}
	}
}
