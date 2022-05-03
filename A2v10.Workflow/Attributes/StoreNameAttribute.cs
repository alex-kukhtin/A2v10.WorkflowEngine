// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class StoreNameAttribute : Attribute
    {
        public String Name { get; }

        public StoreNameAttribute(String name)
        {
            Name = name;
        }
    }
}
