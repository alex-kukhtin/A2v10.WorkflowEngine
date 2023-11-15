// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class StoreNameAttribute(String name) : Attribute
    {
        public String Name { get; } = name;
    }
}
