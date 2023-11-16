// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow
{
    public abstract class StateBase : Activity
    {
        public virtual Boolean IsStart => false;
        public virtual Boolean IsFinal => false;

        public String? Next { get; set; }

        internal String? NextState { get; set; }
    }
}
