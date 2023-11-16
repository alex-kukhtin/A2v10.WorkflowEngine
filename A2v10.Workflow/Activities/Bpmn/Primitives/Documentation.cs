// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using A2v10.System.Xaml;

namespace A2v10.Workflow.Bpmn;

[ContentProperty("Body")]
public class Documentation : BaseElement
{
    public String? Body { get; set; }
}
