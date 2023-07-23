// Copyright © 2021-2023 Oleksandr Kukhtin. All rights reserved.

using System.Collections.Generic;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using A2v10.Data.Interfaces;
using A2v10.Workflow.Engine;
using A2v10.Workflow.Interfaces;
using A2v10.Workflow.Serialization;
using A2v10.WorkflowEngine;


namespace Microsoft.Extensions.DependencyInjection;
public static class WorkflowDependencyInjection
{
    public static IServiceCollection AddWorkflowEngine(this IServiceCollection services, Action<WorkflowEngineOptions>? options = null, Boolean scoped = false)
    {
        services.AddWorkflow();
        if (scoped)
            services.AddSqlServerWorkflowScoped();
        else
            services.AddSqlServerWorkflowSingleton();

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

    public static IServiceCollection AddWorkflowEngineScoped(this IServiceCollection services, Action<WorkflowEngineOptions>? options = null)
    {
        return services.AddWorkflowEngine(options, true);
    }

    public static IServiceCollection AddWorkflowEngineSingleton(this IServiceCollection services, Action<WorkflowEngineOptions>? options = null)
    {
        return services.AddWorkflowEngine(options, false);
    }

    public static IServiceCollection ConfigureWorkflow(this IServiceCollection services, Action<WorkflowStorageOptions> action)
    {
        return services.Configure<WorkflowStorageOptions>(opts =>
        {
            action.Invoke(opts);
        });
    }

    public static IServiceCollection ConfigureWorkflow(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<WorkflowStorageOptions>(opts =>
        {
            var section = configuration.GetSection(WorkflowStoreConfiguration.ConfigurationKey);
            var config = new WorkflowStoreConfiguration();
            section.Bind(config);
            opts.DataSource = config.DataSource;
            opts.MultiTenant = config.MultiTenant;
        });
    }

    public static void CheckStorageVersion(this IServiceProvider services, IHostApplicationLifetime lifetime)
	{
        lifetime.ApplicationStarted.Register(() =>
        {
            using var scope = services.CreateScope();

            var wfStorageVersion = scope.ServiceProvider.GetRequiredService<IWorkflowStorageVersion>()!;
            var wfVersion = wfStorageVersion.GetVersion();
            var logfact = services.GetRequiredService<ILoggerFactory>();
            var logger = logfact.CreateLogger("Startup");
            if (!wfVersion.Valid)
            {
                logger.LogError("Invalid storage version. Required: {Required}, Actual: {Actual}",  wfVersion.Required, wfVersion.Actual);
                lifetime.StopApplication();
            }
            else
            {
                logger.LogInformation("Workflow Storage version: {Actual}", wfVersion.Actual);
            }
        });
    }
}
