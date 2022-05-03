// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;
using A2v10.System.Xaml;

namespace A2v10.Workflow.Bpmn;
[ContentProperty("Children")]
public class BaseElement
{
    public String Id { get; init; } = String.Empty;

    public List<BaseElement>? Children { get; set; }

    public IEnumerable<T> Elems<T>() where T : BaseElement => Children?.OfType<T>() ?? Enumerable.Empty<T>();

    public T? Elem<T>() where T : BaseElement => Children?.OfType<T>().FirstOrDefault();

    public IEnumerable<TResult>? ExtensionElements<TResult>()
    {
        var ee = Children?.OfType<ExtensionElements>().FirstOrDefault();
        return ee?.Items?.OfType<TResult>();
    }
}

