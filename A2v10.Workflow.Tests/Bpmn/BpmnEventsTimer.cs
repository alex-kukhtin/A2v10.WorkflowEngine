﻿// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Workflow.Interfaces;
using System.Dynamic;
using System.Threading;

namespace A2v10.Workflow.Tests
{
	[TestClass]
	[TestCategory("Bmpn.Events.Timer")]
	public class BpmnTimerEvents
	{
		[TestMethod]
		public async Task BoundaryTimer()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\boundary_simple.bpmn");

			var sp = TestEngine.ServiceProvider();

			var wfs = sp.GetRequiredService<IWorkflowStorage>();
			var wfc = sp.GetRequiredService<IWorkflowCatalog>();

			String wfId = "BoundarySimple";
			await wfc.SaveAsync(new WorkflowDescriptor(Id: wfId, Body: xaml, Format: "xaml"));
			var ident = await wfs.PublishAsync(wfc, wfId);

			var wfe = sp.GetRequiredService<IWorkflowEngine>();
			var inst = await wfe.CreateAsync(ident);
			inst = await wfe.RunAsync(inst);
			var res0 = inst.Result;
			Assert.IsNull(res0.Get<String>("Result"));
			Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

			var instTimer = await wfe.HandleEventAsync(inst.Id, "Event1");
			var res1 = instTimer.Result;
			Assert.AreEqual("Timer", res1.Get<String>("Result"));
			Assert.AreEqual(WorkflowExecutionStatus.Idle, instTimer.ExecutionStatus);

			var instResume = await wfe.ResumeAsync(inst.Id, "UserTask1");
			var res2 = instResume.Result;
			Assert.AreEqual("Normal", res2.Get<String>("Result"));
			Assert.AreEqual(WorkflowExecutionStatus.Complete, instResume.ExecutionStatus);
		}

		[TestMethod]
		public async Task IntermediateTimer()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\intermediate_timer.bpmn");

			var sp = TestEngine.ServiceProvider();

			var wfs = sp.GetRequiredService<IWorkflowStorage>();
			var wfc = sp.GetRequiredService<IWorkflowCatalog>();
			var ins = sp.GetRequiredService<IInstanceStorage>();

			String wfId = "IntermediateSimple";
			await wfc.SaveAsync(new WorkflowDescriptor(wfId, xaml));
			var ident = await wfs.PublishAsync(wfc, wfId);

			var wfe = sp.GetRequiredService<IWorkflowEngine>();
			var inst = await wfe.CreateAsync(ident);
			inst = await wfe.RunAsync(inst);
			var res0 = inst.Result;
			Assert.AreEqual("Start", res0.Get<String>("Result"));
			Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

			await wfe.ProcessPending();
			var instAfter = await ins.Load(inst.Id);
			var res1 = instAfter.Result;
			Assert.AreEqual("AfterTimer", res1.Get<String>("Result"));
			Assert.AreEqual(WorkflowExecutionStatus.Complete, instAfter.ExecutionStatus);
		}

		[TestMethod]
		public async Task VariableTimer()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\variable_timer.bpmn");

			String wfId = "VariableTimer";
			var prms = new ExpandoObject()
			{
				{ "Interval", "00:00:01" }
			};

			var wfe = TestEngine.ServiceProvider().GetRequiredService<IWorkflowEngine>();
			var ins = TestEngine.ServiceProvider().GetRequiredService<IInstanceStorage>();

			var inst = await TestEngine.SimpleRun(wfId, xaml, prms);

			Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

			Thread.Sleep(1100);
			await wfe.ProcessPending();

			var instAfter = await ins.Load(inst.Id);
			var res1 = instAfter.Result;
			Assert.AreEqual("AfterTimer", res1.Get<String>("Result"));
			Assert.AreEqual(WorkflowExecutionStatus.Complete, instAfter.ExecutionStatus);
		}

		[TestMethod]
		public async Task IntermediageTimerDate()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\intermediate_timer_date.bpmn");

			String wfId = "Intermediate";

			var now = DateTime.UtcNow + TimeSpan.FromSeconds(1);
			var prms = new ExpandoObject()
			{
				{ "Time", now }
			};

			var wfe = TestEngine.ServiceProvider().GetRequiredService<IWorkflowEngine>();
			var ins = TestEngine.ServiceProvider().GetRequiredService<IInstanceStorage>();

			var inst = await TestEngine.SimpleRun(wfId, xaml, prms);

			Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

			Thread.Sleep(1100);
			await wfe.ProcessPending();

			var instAfter = await ins.Load(inst.Id);
			var res1 = instAfter.Result;
			Assert.AreEqual("AfterTimer", res1.Get<String>("Result"));
			Assert.AreEqual(WorkflowExecutionStatus.Complete, instAfter.ExecutionStatus);
		}
	}
}