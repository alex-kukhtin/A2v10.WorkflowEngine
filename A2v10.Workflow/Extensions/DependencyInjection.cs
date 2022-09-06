
// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.System.Xaml;
using A2v10.Workflow;

namespace Microsoft.Extensions.DependencyInjection;
public static class WorkflowDependencyInjection
{
    public static IServiceCollection AddWorkflow(this IServiceCollection coll)
    {
        coll.AddSingleton<IXamlReaderService, WorkflowXamlReaderService>();
        coll.AddScoped<IWorkflowEngine, WorkflowEngine>();

        coll.AddScoped<ITracker, InstanceTracker>();

        return coll;
    }
}

