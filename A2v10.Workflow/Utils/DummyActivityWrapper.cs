// Copyright © 2020-2026 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow;

internal class DummyActivityWrapper : IActivityWrapper
{
    public T? FindElement<T>(Func<T, bool> predicate) where T : class
    {
        throw new NotImplementedException();
    }

    public IActivity Root()
    {
        throw new NotImplementedException();
    }
}

