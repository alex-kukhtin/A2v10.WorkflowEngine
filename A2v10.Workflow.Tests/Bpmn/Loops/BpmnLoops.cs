﻿// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Workflow.Interfaces;
using System.Dynamic;

namespace A2v10.Workflow.Tests
{
	[TestClass]
	[TestCategory("Bmpn.Loops")]
	public class BpmnLoops
	{
		[TestMethod]
		public async Task SimpleLoop()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\loop\\loop_simple.bpmn");

			String wfId = "SimpleLoop";
			var inst = await TestEngine.SimpleRun(wfId, xaml);

			var res0 = inst.Result;
			Assert.AreEqual(10, res0.Get<Double>("X"));
			Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		}

		[TestMethod]
		public async Task LoopMaximum()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\loop\\loop_maximum.bpmn");

			String wfId = "SimpleLoop";
			var inst = await TestEngine.SimpleRun(wfId, xaml);

			var res0 = inst.Result;
			Assert.AreEqual(12, res0.Get<Double>("X"));
			Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		}

		[TestMethod]
		public async Task LoopTestBefore()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\loop\\loop_testbefore.bpmn");

			String wfId = "SimpleLoop";
			var inst = await TestEngine.SimpleRun(wfId, xaml);

			var res0 = inst.Result;
			Assert.AreEqual(10, res0.Get<Double>("X"));
			Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		}

		[TestMethod]
		public async Task LoopTestNotBefore()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\loop\\loop_testnotbefore.bpmn");

			String wfId = "SimpleLoop";
			var inst = await TestEngine.SimpleRun(wfId, xaml);

			var res0 = inst.Result;
			Assert.AreEqual(9, res0.Get<Double>("X"));
			Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		}

		[TestMethod]
		public async Task LoopWithParallel()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\loop\\loop_withParallel.bpmn");

			String wfId = "LoopWithParallel";
			var inst = await TestEngine.SimpleRun(wfId, xaml);

			var res0 = inst.Result;
			Assert.IsNull(res0.Get<String>("res"));

			Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

			var eng = TestEngine.ServiceProvider().GetService<IWorkflowEngine>();
			var reply = new ExpandoObject()
			{
				{ "Answer", 0 }
			};
			
			var inst2 = await eng.ResumeAsync(inst.Id, "EnterSaldo", reply);
			var res2 = inst2.Result;

			Assert.AreEqual(WorkflowExecutionStatus.Complete, inst2.ExecutionStatus);
		}

	}
}