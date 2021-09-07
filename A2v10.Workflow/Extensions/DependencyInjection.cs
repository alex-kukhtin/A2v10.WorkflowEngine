
// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.System.Xaml;
using A2v10.Workflow;
using A2v10.Workflow.Interfaces;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class DependencyInjection
	{
		public static IServiceCollection UseWorkflow(this IServiceCollection coll)
		{
			coll.AddSingleton<IXamlReaderService, WorkflowXamlReaderService>();
			coll.AddScoped<IWorkflowEngine, WorkflowEngine>()
			.AddScoped<IDeferredTarget, WorkflowDeferred>();

			coll.AddScoped<ITracker, InstanceTracker>();

			return coll;
		}
	}
}
