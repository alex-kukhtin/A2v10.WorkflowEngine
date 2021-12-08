// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.


namespace A2v10.Workflow.Interfaces;

public interface IWorkflowEvent
{
	String Key { get; }
	String Ref { get; }
	ExpandoObject ToExpando();
	ExpandoObject ToStore();
}

