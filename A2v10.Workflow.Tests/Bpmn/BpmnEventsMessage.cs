// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Threading.Tasks;
using System.IO;
using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Tests;
[TestClass]
[TestCategory("Bmpn.Events.Message")]
public class BpmnEventsMessage
{
	[TestMethod]
	public async Task BoundaryUninterrapted()
	{
		var boundaryId = "BoundaryMessage";
		var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\messages\\boundary.bpmn");
		var inst = await TestEngine.SimpleRun(boundaryId, xaml);
		Assert.IsNotNull(inst);
		Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
		var log = inst.Result?.GetNotNull<Object[]>("log");
		Assert.IsNotNull(log);
		Assert.AreEqual(2, log.Length);
		Assert.AreEqual("startProcess|startSub", String.Join('|', log));

		var sp = TestEngine.ServiceProvider();
		var wfe = sp.GetRequiredService<IWorkflowEngine>();
		inst = await wfe.SendMessageAsync(inst.Id, "Message1");
		Assert.IsNotNull(inst);
		Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
		log = inst.Result?.GetNotNull<Object[]>("log");
		Assert.IsNotNull(log);
		Assert.AreEqual(4, log.Length);
		Assert.AreEqual("startProcess|startSub|messageBoundary|endBoundary", String.Join('|', log));

		inst = await wfe.ResumeAsync(inst.Id, "MainTask");
		Assert.IsNotNull(inst);
		Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		log = inst.Result?.GetNotNull<Object[]>("log");
		Assert.IsNotNull(log);
		Assert.AreEqual(6, log.Length);
		Assert.AreEqual("startProcess|startSub|messageBoundary|endBoundary|endSub|endProcess", String.Join('|', log));
	}

	[TestMethod]
	public async Task BoundaryInterrapted()
	{
		var boundaryId = "BoundaryInterruptedMessage";
		var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\messages\\boundary_interrupted.bpmn");
		var inst = await TestEngine.SimpleRun(boundaryId, xaml);
		Assert.IsNotNull(inst);
		Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
		var log = inst.Result?.GetNotNull<Object[]>("log");
		Assert.IsNotNull(log);
		Assert.AreEqual(2, log.Length);
		Assert.AreEqual("startProcess|startSub", String.Join('|', log));

		var sp = TestEngine.ServiceProvider();
		var wfe = sp.GetRequiredService<IWorkflowEngine>();
		inst = await wfe.SendMessageAsync(inst.Id, "Message1");
		Assert.IsNotNull(inst);
		Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		log = inst.Result?.GetNotNull<Object[]>("log");
		Assert.IsNotNull(log);
		Assert.AreEqual(5, log.Length);
		Assert.AreEqual("startProcess|startSub|messageBoundary|endBoundary|endProcess", String.Join('|', log));
	}
}

