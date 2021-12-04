// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;
using System.Dynamic;

using Microsoft.Extensions.DependencyInjection;

using A2v10.Workflow.Interfaces;
using A2v10.Workflow.Serialization;
using A2v10.System.Xaml;

namespace A2v10.Workflow.Tests
{
	public class TestEngine
	{
		public static IWorkflowEngine CreateInMemoryEngine()
		{
			return ServiceProvider().GetService<IWorkflowEngine>() ?? throw new InvalidOperationException(nameof(IWorkflowEngine));
		}

		private static IServiceProvider? _provider;

		public static void Clear()
        {
			_provider = null;
        }

		public static IServiceProvider ServiceProvider()
		{
			if (_provider != null)
				return _provider;

			var collection = new ServiceCollection();

			collection.AddSingleton<IWorkflowStorage, InMemoryWorkflowStorage>();
			collection.AddSingleton<IInstanceStorage, InMemoryInstanceStorage>();
			collection.AddSingleton<IWorkflowCatalog, InMemoryWorkflowCatalog>();
			collection.AddSingleton<IXamlReaderService, WorkflowXamlReaderService>();

			collection.AddSingleton<ISerializer, WorkflowSerializer>();
			collection.AddScoped<IWorkflowEngine, WorkflowEngine>();
			collection.AddScoped<ITracker, ConsoleTracker>();
			collection.AddScoped<IDeferredTarget, WorkflowDeferred>();
			collection.AddScoped<IScriptNativeObjectProvider, ScriptNativeObjects>();

			_provider = collection.BuildServiceProvider();

			return _provider;
		}


		public static async ValueTask<IInstance> SimpleRun(String id, String text, ExpandoObject? prms = null)
		{
			var sp = ServiceProvider();
			var wfs = sp.GetRequiredService<IWorkflowStorage>();
			var wfc = sp.GetRequiredService<IWorkflowCatalog>();

			await wfc.SaveAsync(new WorkflowDescriptor(id, text));
			var ident = await wfs.PublishAsync(wfc, id);

			var wfe = sp.GetRequiredService<IWorkflowEngine>();
			var inst = await wfe.CreateAsync(ident);
			return await wfe.RunAsync(inst, prms);
		}
	}
}
