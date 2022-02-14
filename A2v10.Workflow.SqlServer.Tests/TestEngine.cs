// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;
using System.Dynamic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using A2v10.Workflow.Interfaces;

using A2v10.Data.Interfaces;
using A2v10.Runtime.Interfaces;

using A2v10.WorkflowEngine;

namespace A2v10.Workflow.SqlServer.Tests;
public static class TestEngine
{
	private static IServiceProvider? _provider;


	public static IWorkflowEngine CreateSqlServerEngine()
	{
		return ServiceProvider().GetRequiredService<IWorkflowEngine>();
	}

	private readonly static NativeType[] _nativeTypes = new NativeType[]
	{
		new NativeType(Name: "External", Type:typeof(TestExternalNatvieType))
	};

	public static IServiceProvider ServiceProvider()
	{
		if (_provider != null)
			return _provider;

		var collection = new ServiceCollection();
		collection.AddSingleton<IConfiguration>(TestConfig.GetRoot());

		collection.AddWorkflowEngine(options =>
        {
			options.NativeTypes = _nativeTypes;
        });
		collection.UseSimpleDbContext();
		collection.AddSingleton<IRuntimeInvokeTarget, WorkflowInvokeTarget>();

		_provider = collection.BuildServiceProvider();

		return _provider;
	}


	public static Task PrepareDatabase(String id)
	{
		var dbContext = ServiceProvider().GetRequiredService<IDbContext>();
		var prms = new ExpandoObject()
		{
			{ "Id", id }
		};
		return dbContext.ExecuteExpandoAsync(null, "a2wf_test.[Tests.Prepare]", prms);
	}

	public static async ValueTask<IInstance> SimpleRun(String id, String text, ExpandoObject? prms = null, String? correlationId = null)
	{
		var sp = ServiceProvider();
		var wfs = sp.GetRequiredService<IWorkflowStorage>();
		var wfc = sp.GetRequiredService<IWorkflowCatalog>();

		await wfc.SaveAsync(new WorkflowDescriptor(id, text));
		var ident = await wfs.PublishAsync(wfc, id);

		var wfe = sp.GetRequiredService<IWorkflowEngine>();
		var inst = await wfe.CreateAsync(ident, correlationId);
		return await wfe.RunAsync(inst, prms);
	}

}

