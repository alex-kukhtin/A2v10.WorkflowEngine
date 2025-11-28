// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.


using A2v10.System.Xaml;
using A2v10.Workflow.Bpmn;

namespace A2v10.Workflow;

/*two classes with same name is required !*/
[ContentProperty("Text")]
public class Track : BaseElement
{
    public String? Text { get; init; }
}

