// Copyright © 2021-2022 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;

using A2v10.Workflow.Interfaces;
using A2v10.Workflow.Serialization;
using A2v10.WorkflowEngine;

namespace Microsoft.Extensions.DependencyInjection;
public static class WorkflowDependencyInjection
{
	public static IServiceCollection UseWorkflowEngine(this IServiceCollection services)
	{
		services.UseWorkflow();
		services.UseSqlServerWorkflow();

		services.AddSingleton<ISerializer, WorkflowSerializer>();

		services.AddSingleton<IDbIdentity, DbIdentity>();
		services.AddSingleton<IScriptNativeObjectProvider, AppScriptNativeObjects>();

		return services;
	}
}

