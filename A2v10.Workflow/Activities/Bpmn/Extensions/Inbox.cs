// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.


using A2v10.System.Xaml;

using A2v10.Workflow.Bpmn;

namespace A2v10.Workflow;

/*two classes with same name is required !*/
[ContentProperty("Text")]
public class Inbox : BaseElement
{
	public String? Text { get; init; }
}

