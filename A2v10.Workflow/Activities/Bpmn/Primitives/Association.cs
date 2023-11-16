// Copyright © 2021 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Bpmn;
public class Association : BaseElement
{
    public String? SourceRef { get; init; }
    public String? TargetRef { get; init; }
}
