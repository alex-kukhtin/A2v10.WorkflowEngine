// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Tests
{
	[TestClass]
	[TestCategory("Bmpn.Loops")]
	public class BpmnLoops
	{
		[TestMethod]
		public async Task SimpleLoop()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\loop_simple.bpmn");

			String wfId = "SimpleLoop";
			var inst = await TestEngine.SimpleRun(wfId, xaml);

			var res0 = inst.Result;
			Assert.AreEqual(10, res0.Get<Double>("X"));
			Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		}

		[TestMethod]
		public async Task LoopMaximum()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\loop_maximum.bpmn");

			String wfId = "SimpleLoop";
			var inst = await TestEngine.SimpleRun(wfId, xaml);

			var res0 = inst.Result;
			Assert.AreEqual(12, res0.Get<Double>("X"));
			Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		}

		[TestMethod]
		public async Task LoopTestBefore()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\loop_testbefore.bpmn");

			String wfId = "SimpleLoop";
			var inst = await TestEngine.SimpleRun(wfId, xaml);

			var res0 = inst.Result;
			Assert.AreEqual(12, res0.Get<Double>("X"));
			Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		}
	}
}
