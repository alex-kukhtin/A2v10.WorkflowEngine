// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Workflow.Interfaces;
using System.Dynamic;

namespace A2v10.Workflow.SqlServer.Tests;

[TestClass]
[TestCategory("Storage.NativeObjects")]
public class NativeObjects
{
	private readonly IServiceProvider _serviceProvider;
	private readonly IWorkflowStorage _workflowStorage;
	private readonly IWorkflowCatalog _workflowCatalog;
	private readonly IWorkflowEngine _workflowEngine;

	public NativeObjects()
	{
		_serviceProvider = TestEngine.ServiceProvider();
		_workflowStorage = _serviceProvider.GetRequiredService<IWorkflowStorage>();
		_workflowCatalog = _serviceProvider.GetRequiredService<IWorkflowCatalog>();
		_workflowEngine = _serviceProvider.GetRequiredService<IWorkflowEngine>();
    }


    [TestMethod]
    public async Task SimpleNative()
    {
		String wfId = "simpleNative";

		await TestEngine.PrepareDatabase(wfId);
		var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\native\\native_simple.bpmn");
		var inst = await TestEngine.SimpleRun(wfId, xaml);

		Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);

		var res = await _workflowEngine.LoadInstanceRaw(inst.Id);

		var res0 = res.Result;
		var log = inst.Result?.GetNotNull<Object[]>("log");
		Assert.IsNotNull(log);
		Assert.AreEqual(5, log.Length);
		Assert.AreEqual("start|task|res:5|success:true|end", String.Join('|', log));
	}
}

