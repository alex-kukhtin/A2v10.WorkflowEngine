// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

using A2v10.System.Xaml;

namespace A2v10.Workflow.Bpmn
{
	[ContentProperty("Children")]
	public class BaseElement
	{
		public String Id { get; init; }

		public List<BaseElement> Children { get; set; }

		public IEnumerable<T> Elems<T>() where T : BaseElement => Children?.OfType<T>() ?? Enumerable.Empty<T>();

		public T Elem<T>() where T : BaseElement => Children?.OfType<T>().FirstOrDefault();

		public IEnumerable<TResult> ExtensionElements<TResult>()
		{
			var ee = Children?.OfType<ExtensionElements>().FirstOrDefault();
			return ee?.Items?.OfType<TResult>();
		}
	}
}
