// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;
using System.Dynamic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using A2v10.System.Xaml;
using A2v10.Workflow.Interfaces;
using A2v10.Workflow.Serialization;

using A2v10.Data.Extensions;
using A2v10.Data.Interfaces;

namespace A2v10.Workflow.SqlServer.Tests
{
	public static class TestEngine
	{
		private static IServiceProvider _provider;


		public static IWorkflowEngine CreateSqlServerEngine()
		{
			return ServiceProvider().GetService<IWorkflowEngine>();
		}

		public static IServiceProvider ServiceProvider()
		{
			if (_provider != null)
				return _provider;

			var collection = new ServiceCollection();
			collection.AddSingleton<IConfiguration>(TestConfig.GetRoot());

			collection.UseSimpleDbContext();

			collection.AddSingleton<IDbIdentity, UserIdentity>();

			collection.AddSingleton<IWorkflowStorage, SqlServerWorkflowStorage>()
			.AddSingleton<IInstanceStorage, SqlServerInstanceStorage>()
			.AddSingleton<IWorkflowCatalog, SqlServerWorkflowCatalog>()
			.AddSingleton<IXamlReaderService, WorkflowXamlReaderService>();

			collection.AddSingleton<ISerializer, WorkflowSerializer>()
			.AddScoped<IWorkflowEngine, WorkflowEngine>()
			.AddScoped<IDeferredTarget, WorkflowDeferred>();

			collection.AddScoped<ITracker, InstanceTracker>();

			_provider = collection.BuildServiceProvider();

			return _provider;
		}


		public static Task PrepareDatabase(String id)
		{
			var dbContext = ServiceProvider().GetService<IDbContext>();
			var prms = new ExpandoObject()
			{
				{ "Id", id }
			};
			return dbContext.ExecuteExpandoAsync(null, "a2wf_test.[Tests.Prepare]", prms);
		}
	}
}
