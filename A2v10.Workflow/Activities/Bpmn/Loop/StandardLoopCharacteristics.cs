// Copyright © 2021 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Bpmn;

public class StandardLoopCharacteristics : BaseElement
{
    public Boolean TestBefore { get; init; }
    public Int32 LoopMaximum { get; init; }

    public String? LoopCondition => Children?.OfType<LoopCondition>().FirstOrDefault()?.Expression;
}

