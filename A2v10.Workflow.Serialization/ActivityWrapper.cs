// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Serialization
{
    public record ActivityWrapper
    {
        public IActivity? Root { get; init; }
    }
}
