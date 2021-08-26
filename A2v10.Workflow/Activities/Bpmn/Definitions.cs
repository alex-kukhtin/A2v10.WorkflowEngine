// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

using A2v10.System.Xaml;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	[ContentProperty("Children")]
	public class Definitions : IActivityWrapper
	{
		public String Id { get; init; }
		public String Name { get; init; }
		public String TargetNamespace { get; init; }
		public String Exporter { get; set; }
		public String ExporterVersion { get; set; }
		public String ExpressionLanguage { get; set; }
		public String TypeLanguage { get; set; }

		public List<Object> Children { get; init; }

		public IActivity Root() => GetRoot();

		public Process Process => Children.OfType<Process>().FirstOrDefault();

		public IActivity GetRoot()
		{
			var collaboration = Children.OfType<Collaboration>().FirstOrDefault();
			if (collaboration != null)
			{

				collaboration.AddProcesses(Children.OfType<Process>());
				return collaboration;
			}
			return Process;
		}
	}
}
