﻿// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using A2v10.System.Xaml;

namespace A2v10.Workflow.Bpmn
{
    /*two classes with same name is required !*/
    [ContentProperty("Text")]
    public class Script : BaseElement
    {
        public String? Text { get; init; }
    }
}
