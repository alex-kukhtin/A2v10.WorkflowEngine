// Copyright © 2020-2022 Oleksandr Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using A2v10.Workflow.SqlServer;

namespace Microsoft.Extensions.DependencyInjection;
public static class WorkflowSqlDependencyInjection
{
    public static IServiceCollection AddSqlServerWorkflowScoped(this IServiceCollection coll)
    {
        coll.AddScoped<IWorkflowStorageVersion, SqlServerStorageVersion>()
        .AddScoped<IWorkflowStorage, SqlServerWorkflowStorage>()
        .AddScoped<IInstanceStorage, SqlServerInstanceStorage>()
        .AddScoped<IWorkflowCatalog, SqlServerWorkflowCatalog>()
        .AddScoped<IDataSourceProvider, DataSourceProviderScoped>();
        return coll;
    }

    public static IServiceCollection AddSqlServerWorkflowSingleton(this IServiceCollection coll)
    {
        coll.AddSingleton<IWorkflowStorageVersion, SqlServerStorageVersion>()
        .AddSingleton<IWorkflowStorage, SqlServerWorkflowStorage>()
        .AddSingleton<IInstanceStorage, SqlServerInstanceStorage>()
        .AddSingleton<IWorkflowCatalog, SqlServerWorkflowCatalog>()
        .AddSingleton<IDataSourceProvider, DataSourceProviderSingleton>();
        return coll;
    }
}