// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	public static class WorkflowEventImpl
	{
		public static IWorkflowEvent FromExpando(String key, ExpandoObject exp)
		{
			var kind = exp.Get<String>("Kind");
			return kind switch
			{
				"Timer" => new WorkflowTimerEvent(key, exp),
				_ => throw new WorkflowException($"Invalid event kind ({kind})"),
			};
		}
	}
}
