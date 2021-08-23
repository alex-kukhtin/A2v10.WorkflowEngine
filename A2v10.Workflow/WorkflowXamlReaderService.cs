// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.System.Xaml;

namespace A2v10.Workflow
{
	public class WorkflowXamlReaderService : XamlReaderService
	{
		public override XamlServicesOptions Options { get; set; }

		public WorkflowXamlReaderService()
		{
			Options = XamlServicesOptions.BpmnXamlOptions;
		}
	}
}
