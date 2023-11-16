// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow
{
    public abstract class FlowNode : Activity
    {
        public virtual Boolean IsStart => false;

        public String? Next { get; set; }

        internal Flowchart ParentFlow => Parent as Flowchart ?? throw new ArgumentNullException(nameof(Parent));
    }
}
