// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;
using System.Dynamic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Tests;

[TestClass]
[TestCategory("Bpmn.CallActitity")]
public class BpmnCallActivity
{
	[TestMethod]
	public async Task SimpleCall()
	{
		TestEngine.Clear();
		var sp = TestEngine.ServiceProvider();
		var wfs = sp.GetRequiredService<IWorkflowStorage>();
		var wfc = sp.GetRequiredService<IWorkflowCatalog>();

		String childId = "SimpleChild";
		var xamlChild = File.ReadAllText("..\\..\\..\\TestFiles\\CallActivity\\SimpleChild.bpmn");
		await wfc.SaveAsync(new WorkflowDescriptor(childId, xamlChild));
		var ident = await wfs.PublishAsync(wfc, childId);
		Assert.AreEqual(1, ident.Version);

		String parentId = "SimpleParent";
		var xamlParent = File.ReadAllText("..\\..\\..\\TestFiles\\CallActivity\\SimpleParent.bpmn");
		var prms = new ExpandoObject()
		{
			{"A", 2 },
			{"B", 2 },
		};
		var inst = await TestEngine.SimpleRun(parentId, xamlParent, prms);

		var res0 = inst.Result;
		Assert.AreEqual(4, res0.Get<Double>("R"));
		Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
	}

	[TestMethod]
	public async Task CallWithBookmark()
	{
		TestEngine.Clear();
		var sp = TestEngine.ServiceProvider();
		var wfs = sp.GetRequiredService<IWorkflowStorage>();
		var wfc = sp.GetRequiredService<IWorkflowCatalog>();
		var wfe = sp.GetRequiredService<IWorkflowEngine>();
		var ist = sp.GetRequiredService<IInstanceStorage>();

		String childId = "SimpleChild";
		var xamlChild = File.ReadAllText("..\\..\\..\\TestFiles\\CallActivity\\SimpleChildTimer.bpmn");
		await wfc.SaveAsync(new WorkflowDescriptor(childId, xamlChild));
		var ident = await wfs.PublishAsync(wfc, childId);
		Assert.AreEqual(1, ident.Version);

		String parentId = "SimpleParent";
		var xamlParent = File.ReadAllText("..\\..\\..\\TestFiles\\CallActivity\\SimpleParent.bpmn");
		var prms = new ExpandoObject()
		{
			{"A", 2 },
			{"B", 2 },
		};
		var inst = await TestEngine.SimpleRun(parentId, xamlParent, prms);

		Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

		await Task.Delay(1010);
		await wfe.ProcessPending();

		var res = await ist.Load(inst.Id);
		
		var res0 = res.Result;
		Assert.AreEqual(4, res0.Get<Double>("R"));
	}


	[TestMethod]
	public async Task SimpleEmpty()
	{
		TestEngine.Clear();
		var sp = TestEngine.ServiceProvider();
		var wfs = sp.GetRequiredService<IWorkflowStorage>();
		var wfc = sp.GetRequiredService<IWorkflowCatalog>();

		String childId = "SimpleChild";
		var xamlChild = File.ReadAllText("..\\..\\..\\TestFiles\\CallActivity\\SimpleChild.bpmn");
		await wfc.SaveAsync(new WorkflowDescriptor(childId, xamlChild));
		var ident = await wfs.PublishAsync(wfc, childId);
		Assert.AreEqual(1, ident.Version);

		String parentId = "SimpleParent";
		var xamlParent = File.ReadAllText("..\\..\\..\\TestFiles\\CallActivity\\SimpleEmpty.bpmn");
		var inst = await TestEngine.SimpleRun(parentId, xamlParent, null);

		var res0 = inst.Result;
		Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		Assert.IsNull(res0.Get<Object>("R"));
	}
}

