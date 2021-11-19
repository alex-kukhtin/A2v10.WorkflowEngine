// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	public class EventDefinition : BpmnActivity
	{
		public virtual IWorkflowEvent CreateEvent(String id, IExecutionContext context)
		{
			return null;
		}

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token)
		{
			throw new NotImplementedException();
		}

		public virtual Boolean CanRepeat { get; }
	}
}
