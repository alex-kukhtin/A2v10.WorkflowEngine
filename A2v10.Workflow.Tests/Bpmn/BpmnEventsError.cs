// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Tests;
[TestClass]
[TestCategory("Bmpn.Events.Error")]

public class BpmnErrorEvents
{
	[TestMethod]
	public async Task Simple()
	{
		var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\error\\error1.bpmn");

		String wfId = "Error1";
		var inst = await TestEngine.SimpleRun(wfId, xaml);

		Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		var log = inst.Result?.GetNotNull<Object[]>("log");
		Assert.IsNotNull(log);
		//Assert.AreEqual(6, log.Length);
		Assert.AreEqual("start|task1|endError|endBoundary", String.Join('|', log));
	}
}

