// Copyright © 2021-2022 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;

using A2v10.Data.Interfaces;
using A2v10.Workflow.Engine;
using A2v10.Workflow.Interfaces;
using A2v10.Workflow.Serialization;
using A2v10.WorkflowEngine;

namespace Microsoft.Extensions.DependencyInjection;
public static class WorkflowDependencyInjection
{
	public static IServiceCollection AddWorkflowEngine(this IServiceCollection services, Action<WorkflowEngineOptions>? options = null)
	{
		services.AddWorkflow();
		services.AddSqlServerWorkflow();

		services.AddSingleton<ISerializer, WorkflowSerializer>();

		services.AddSingleton<IDbIdentity, DbIdentity>();

		IEnumerable<NativeType>? nativeTypes = null;
		if (options != null)
		{
			var opts = new WorkflowEngineOptions();
			options?.Invoke(opts);
			nativeTypes = opts.NativeTypes;
		}

		services.AddSingleton<IScriptNativeObjectProvider>(sp =>
        {
			return new AppScriptNativeObjects(nativeTypes);
        });

		return services;
	}
}

