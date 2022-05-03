// Copyright © 2021 Alex Kukhtin. All rights reserved.

using A2v10.System.Xaml;

namespace A2v10.Workflow.Bpmn
{
    public class TextAnnotation : BaseElement
    {
    }

    [ContentProperty("Body")]
    public class Text : BaseElement
    {
        public String? Body { get; set; }
    }
}
