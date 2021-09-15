// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using A2v10.Workflow.SqlServer;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class WorkflowSqlDependencyInjection
	{
		public static IServiceCollection UseSqlServerWorkflow(this IServiceCollection coll)
		{
			coll.UseWorkflow();

			coll.AddSingleton<IWorkflowStorage, SqlServerWorkflowStorage>()
			.AddSingleton<IInstanceStorage, SqlServerInstanceStorage>()
			.AddSingleton<IWorkflowCatalog, SqlServerWorkflowCatalog>();
			return coll;
		}
	}
}
