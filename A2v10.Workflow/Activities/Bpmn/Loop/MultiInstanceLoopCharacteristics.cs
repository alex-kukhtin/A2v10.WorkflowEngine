// Copyright © 2022 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Bpmn;

public class MultiInstanceLoopCharacteristics : BaseElement
{
    public Boolean IsSequential { get; init; }
    public String? Collection { get; init; }
    public String? Variable { get; init; }
}

