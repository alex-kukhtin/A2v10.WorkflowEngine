
// Copyright © 2020-2022 Oleksandr Kukhtin. All rights reserved.

using A2v10.System.Xaml;
using A2v10.Workflow;

namespace Microsoft.Extensions.DependencyInjection;
public static class WorkflowDependencyInjection
{
    public static IServiceCollection AddWorkflow(this IServiceCollection coll)
    {
        return coll
            .AddSingleton<IXamlReaderService, WorkflowXamlReaderService>()
            .AddScoped<IWorkflowEngine, WorkflowEngine>()
            .AddScoped<ITracker, InstanceTracker>();
    }
}

