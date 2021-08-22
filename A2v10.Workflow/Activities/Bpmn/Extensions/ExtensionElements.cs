// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

using A2v10.System.Xaml;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	[ContentProperty("Items")]
	public class ExtensionElements : BaseElement
	{
		public List<BaseElement> Items { get; init; }

		public List<IVariable> GetVariables()
		{
			return Items?.OfType<Variables>().FirstOrDefault()?.Items?.OfType<IVariable>()?.ToList();
		}

		public String GetGlobalScript()
		{
			var scriptElem = Items.OfType<GlobalScript>().FirstOrDefault();
			return scriptElem?.Text;
		}
	}
}
