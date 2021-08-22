// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;

namespace A2v10.Workflow.Interfaces
{
	public interface IWorkflowEvent
	{
		String Key { get; }
		ExpandoObject ToExpando();
		ExpandoObject ToStore();
	}
}
