// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	public class EventDefinition : BaseElement
	{
		public virtual IWorkflowEvent CreateEvent(String id)
		{
			return null;
		}

		public virtual Boolean CanRepeat { get; }
	}
}
