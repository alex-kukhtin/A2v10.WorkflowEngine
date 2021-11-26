// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Tests
{
	[TestClass]
	[TestCategory("Bmpn.ServiceTasks")]
	public class BpmnServiceTasks
	{
		[TestMethod]
		public async Task CallApi()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\servicetask\\callapi.bpmn");
			String wfId = "CallApi";
			var inst = await TestEngine.SimpleRun(wfId, xaml, null);

			//var res0 = inst.Result;
			Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		}

		[TestMethod]
		public async Task CallActivity()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\servicetask\\callactivity.bpmn");
			String wfId = "CallActivity";
			var inst = await TestEngine.SimpleRun(wfId, xaml, null);

			//var res0 = inst.Result;
			Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		}
	}
}
