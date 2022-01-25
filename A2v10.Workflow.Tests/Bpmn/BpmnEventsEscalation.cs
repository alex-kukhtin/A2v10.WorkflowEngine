// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Tests;
[TestClass]
[TestCategory("Bpmn.Events.Escalation")]

public class BpmnEscalationEvents
{
	[TestMethod]
	public async Task Simple()
	{
		var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\escalation\\escalation1.bpmn");

		String wfId = "Escalation1";
		var inst = await TestEngine.SimpleRun(wfId, xaml);

		Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		var log = inst.Result?.GetNotNull<Object[]>("log");
		Assert.IsNotNull(log);
		//Assert.AreEqual(6, log.Length);
		Assert.AreEqual("start|task1|endEscalation|endBoundary", String.Join('|', log));
	}

	[TestMethod]
	public async Task SubProcess()
	{
		var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\escalation\\escalation_sub.bpmn");

		String wfId = "Escalation1";
		var inst = await TestEngine.SimpleRun(wfId, xaml);

		Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		var log = inst.Result?.GetNotNull<Object[]>("log");
		Assert.IsNotNull(log);
		//Assert.AreEqual(6, log.Length);
		Assert.AreEqual("start|startSub|endEscalationSub|endEscalation", String.Join('|', log));
	}


	[TestMethod]
	public async Task ParentChild()
	{
		String childId = "ecalation_child";
		var xamlchild = File.ReadAllText("..\\..\\..\\TestFiles\\events\\escalation\\escalation_child.bpmn");
		var ident = await TestEngine.SimplePublish(childId, xamlchild);
		Assert.AreEqual(1, ident.Version);

		var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\escalation\\escalation_parent.bpmn");

		String wfId = "EscalationParent";
		var inst = await TestEngine.SimpleRun(wfId, xaml);

		Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		var log = inst.Result?.GetNotNull<Object[]>("log");
		Assert.IsNotNull(log);
		//Assert.AreEqual(6, log.Length);
		Assert.AreEqual("start|endEscalation", String.Join('|', log));
	}

	[TestMethod]
	public async Task ParentChildTimer()
	{
		String childId = "escalation_child_timer";
		var xamlchild = File.ReadAllText("..\\..\\..\\TestFiles\\events\\escalation\\escalation_child_timer.bpmn");
		var ident = await TestEngine.SimplePublish(childId, xamlchild);
		Assert.AreEqual(1, ident.Version);

		var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\escalation\\escalation_parent_timer.bpmn");

		String wfId = "EscalationParent";
		var inst = await TestEngine.SimpleRun(wfId, xaml);

		Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
		var log = inst.Result?.GetNotNull<Object[]>("log");
		Assert.IsNotNull(log);
		Assert.AreEqual(1, log.Length);
		Assert.AreEqual("start", String.Join('|', log));

		await Task.Delay(1010);
		var wfe = TestEngine.ServiceProvider().GetRequiredService<IWorkflowEngine>();
		await wfe.ProcessPending();

		var inst2 = await wfe.LoadInstanceRaw(inst.Id);
		Assert.AreEqual(WorkflowExecutionStatus.Complete, inst2.ExecutionStatus);
		log = inst2.Result?.GetNotNull<Object[]>("log");
		Assert.IsNotNull(log);
		Assert.AreEqual(2, log.Length);
		Assert.AreEqual("start|endBoundary", String.Join('|', log));

	}
}

