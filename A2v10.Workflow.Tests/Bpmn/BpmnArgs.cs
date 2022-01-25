// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;
using System.Dynamic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Tests
{
	[TestClass]
	[TestCategory("Bpmn.Args")]
	public class BpmnArgs
	{
		[TestMethod]
		public async Task ArgTypes()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\args_types.bpmn");

			var sp = TestEngine.ServiceProvider();

			var wfs = sp.GetRequiredService<IWorkflowStorage>();
			var wfc = sp.GetRequiredService<IWorkflowCatalog>();

			String wfId = "ArgTypes";
			await wfc.SaveAsync(new WorkflowDescriptor(Id: wfId, Body: xaml, Format:"xaml")
			{
				Id = wfId,
				Body = xaml,
				Format = "xaml"
			});
			var ident = await wfs.PublishAsync(wfc, wfId);

			var wfe = sp.GetRequiredService<IWorkflowEngine>();
			var inst = await wfe.CreateAsync(ident);
			var now = DateTime.UtcNow;
			inst = await wfe.RunAsync(inst, new ExpandoObject() {
				{"X", "5" },
				{"S",  8 },
				{ "B", "true" },
				{ "I", 77 },
				{ "D", now }
			});
			var res0 = inst.Result;
			Assert.AreEqual(10, res0.Get<Double>("X"));
			Assert.AreEqual("88", res0.Get<String>("S"));
			Assert.AreEqual(false, res0.Get<Boolean>("B"));
			Assert.AreEqual(77, res0.Get<Int64>("I"));
			var rdDate = res0.Get<DateTime>("D");
			Assert.AreEqual(0, (Int32)(rdDate - now).TotalMilliseconds);
			Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		}

		[TestMethod]
		public async Task ArgOnlyOutput()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\args_onlyoutput.bpmn");
			String wfId = "OnlyArgs";
			var prms = new ExpandoObject()
			{
				{"S", "VALUE" }
			};
			var inst = await TestEngine.SimpleRun(wfId, xaml, prms);

			var res0 = inst.Result;
			Assert.AreEqual("OUTPUT", res0.Get<String>("S"));
			Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		}

		[TestMethod]
		public async Task ArgEmptyObject()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\args_types.bpmn");
			String wfId = "ArgEmptyObject";
			var prms = new ExpandoObject()
			{
			};
			var inst = await TestEngine.SimpleRun(wfId, xaml, prms);

			var res0 = inst.Result;
			Assert.AreEqual(true, res0.Get<Boolean>("B"));
			Assert.AreEqual(106, res0.Get<Int64>("I"));
			Assert.AreEqual("S8", res0.Get<String>("S"));
			Assert.AreEqual(7 + 5, res0.Get<Double>("X"));
			var rDate = res0.Get<DateTime>("D");
			Assert.AreEqual(0, (Int32) (new DateTime(2008, 9, 22, 14, 01, 54, DateTimeKind.Utc) - rDate).TotalMilliseconds);
			Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		}
	}
}
