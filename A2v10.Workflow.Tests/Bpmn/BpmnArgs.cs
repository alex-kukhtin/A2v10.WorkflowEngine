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
	[TestCategory("Bmpn.Args")]
	public class BpmnArgs
	{
		[TestMethod]
		public async Task ArgTypes()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\args_types.bpmn");

			var sp = TestEngine.ServiceProvider();

			var wfs = sp.GetService<IWorkflowStorage>();
			var wfc = sp.GetService<IWorkflowCatalog>();

			String wfId = "ArgTypes";
			await wfc.SaveAsync(new WorkflowDescriptor()
			{
				Id = wfId,
				Body = xaml,
				Format = "xaml"
			});
			var ident = await wfs.PublishAsync(wfc, wfId);

			var wfe = sp.GetService<IWorkflowEngine>();
			var inst = await wfe.CreateAsync(ident);
			inst = await wfe.RunAsync(inst, new ExpandoObject() {
				{"X", "5" },
				{"S",  8 },
				{ "B", "true" }
			});
			var res0 = inst.Result;
			Assert.AreEqual(10, res0.Get<Double>("X"));
			Assert.AreEqual("88", res0.Get<String>("S"));
			Assert.AreEqual(false, res0.Get<Boolean>("B"));
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
			Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
		}
	}
}
