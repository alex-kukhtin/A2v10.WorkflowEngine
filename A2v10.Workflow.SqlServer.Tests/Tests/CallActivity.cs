// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;
using System.Dynamic;
using Newtonsoft.Json;
using A2v10.Data;

namespace A2v10.Workflow.SqlServer.Tests;

[TestClass]
[TestCategory("Storage.CallActivity")]
public class CallActivity
{
	private readonly IServiceProvider _serviceProvider;
	private readonly IWorkflowStorage _workflowStorage;
	private readonly IWorkflowCatalog _workflowCatalog;
	private readonly IWorkflowEngine _workflowEngine;
	private readonly IDbContext _dbContext;

	public CallActivity()
	{
		_serviceProvider = TestEngine.ServiceProvider();
		_workflowStorage = _serviceProvider.GetRequiredService<IWorkflowStorage>();
		_workflowCatalog = _serviceProvider.GetRequiredService<IWorkflowCatalog>();
		_workflowEngine = _serviceProvider.GetRequiredService<IWorkflowEngine>();
		_dbContext = _serviceProvider.GetRequiredService<IDbContext>();
    }


    [TestMethod]
    public async Task SimpleWithTimer()
    {
		String parentId = "SimpleParent";
		String childId = "SimpleChild";

		await TestEngine.PrepareDatabase(childId);
		await TestEngine.PrepareDatabase(parentId);
		var sp = TestEngine.ServiceProvider();
		var wfs = sp.GetRequiredService<IWorkflowStorage>();
		var wfc = sp.GetRequiredService<IWorkflowCatalog>();
		var wfe = sp.GetRequiredService<IWorkflowEngine>();
		var ist = sp.GetRequiredService<IInstanceStorage>();

		var xamlChild = File.ReadAllText("..\\..\\..\\TestFiles\\CallActivity\\SimpleChildTimer.bpmn");
		await wfc.SaveAsync(new WorkflowDescriptor(childId, xamlChild));
		var childIdent = await wfs.PublishAsync(wfc, childId);
		Assert.AreEqual(1, childIdent.Version);

		var xamlParent = File.ReadAllText("..\\..\\..\\TestFiles\\CallActivity\\SimpleParent.bpmn");
		await wfc.SaveAsync(new WorkflowDescriptor(parentId, xamlParent));
		var parentIdent = await wfs.PublishAsync(wfc, parentId);
		Assert.AreEqual(1, parentIdent.Version);

		var prms = new ExpandoObject()
		{
			{"A", 2 },
			{"B", 2 },
		};
		var inst = await wfe.CreateAsync(new WorkflowIdentity(parentId));
		inst = await wfe.RunAsync(inst.Id, prms);

		Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

		await Task.Delay(1010);
		await wfe.ProcessPending();

		var res = await wfe.LoadInstance(inst.Id);

		var res0 = res.Result;
		Assert.AreEqual(4, res0.Get<Double>("R"));
	}
}

