// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using A2v10.Workflow.SqlServer;

namespace Microsoft.Extensions.DependencyInjection;
public static class WorkflowSqlDependencyInjection
{
	public static IServiceCollection UseSqlServerWorkflow(this IServiceCollection coll)
	{
		coll.UseWorkflow();

		coll.AddScoped<IWorkflowStorage, SqlServerWorkflowStorage>()
		.AddScoped<IInstanceStorage, SqlServerInstanceStorage>()
		.AddScoped<IWorkflowCatalog, SqlServerWorkflowCatalog>();
		return coll;
	}
}