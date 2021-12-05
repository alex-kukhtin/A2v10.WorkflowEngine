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

	public CallActivity()
	{
		_serviceProvider = TestEngine.ServiceProvider();
		_workflowStorage = _serviceProvider.GetRequiredService<IWorkflowStorage>();
		_workflowCatalog = _serviceProvider.GetRequiredService<IWorkflowCatalog>();
		_workflowEngine = _serviceProvider.GetRequiredService<IWorkflowEngine>();
    }


    [TestMethod]
    public async Task SimpleWithTimer()
    {
		String parentId = "SimpleParent";
		String childId = "SimpleChild";

		await TestEngine.PrepareDatabase(childId);
		await TestEngine.PrepareDatabase(parentId);

		var xamlChild = File.ReadAllText("..\\..\\..\\TestFiles\\CallActivity\\SimpleChildTimer.bpmn");
		await _workflowCatalog.SaveAsync(new WorkflowDescriptor(childId, xamlChild));
		var childIdent = await _workflowStorage.PublishAsync(_workflowCatalog, childId);
		Assert.AreEqual(1, childIdent.Version);

		var xamlParent = File.ReadAllText("..\\..\\..\\TestFiles\\CallActivity\\SimpleParent.bpmn");
		await _workflowCatalog.SaveAsync(new WorkflowDescriptor(parentId, xamlParent));
		var parentIdent = await _workflowStorage.PublishAsync(_workflowCatalog, parentId);
		Assert.AreEqual(1, parentIdent.Version);

		var prms = new ExpandoObject()
		{
			{"A", 2 },
			{"B", 2 },
		};
		var inst = await _workflowEngine.CreateAsync(new WorkflowIdentity(parentId));
		inst = await _workflowEngine.RunAsync(inst.Id, prms);

		Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

		await Task.Delay(1010);
		await _workflowEngine.ProcessPending();

		var res = await _workflowEngine.LoadInstance(inst.Id);

		var res0 = res.Result;
		Assert.AreEqual(4, res0.Get<Double>("R"));
	}
}

