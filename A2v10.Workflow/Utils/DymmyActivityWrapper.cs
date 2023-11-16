// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow;

internal class DymmyActivityWrapper : IActivityWrapper
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

