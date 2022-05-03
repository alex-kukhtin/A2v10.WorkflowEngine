// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;

public interface IActivityWrapper
{
    IActivity Root();
    public T? FindElement<T>(Func<T, Boolean> predicate) where T : class;
}

