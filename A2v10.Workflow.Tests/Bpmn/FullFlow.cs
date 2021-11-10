// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Tests
{
	[TestClass]
	[TestCategory("Bmpn.Full")]
	public class FullFlow
	{
		[TestMethod]
		public async Task Parallel()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\parallel_1.bpmn");

			var sp = TestEngine.ServiceProvider();

			var wfs = sp.GetService<IWorkflowStorage>();
			var wfc = sp.GetService<IWorkflowCatalog>();

			String wfId = "Parallel1";
			await wfc.SaveAsync(new WorkflowDescriptor()
			{
				Id = wfId,
				Body = xaml,
				Format = "xaml"
			});
			var ident = await wfs.PublishAsync(wfc, wfId);

			var wfe = sp.GetService<IWorkflowEngine>();
			var inst = await wfe.CreateAsync(ident);
			inst = await wfe.RunAsync(inst, new { X = 5 });
			var res = inst.Result;

			Assert.AreEqual(12, res.Get<Int32>("X"));
		}

		[TestMethod]
		public async Task UserTask()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\user_task_1.bpmn");

			var sp = TestEngine.ServiceProvider();

			var wfs = sp.GetService<IWorkflowStorage>();
			var wfc = sp.GetService<IWorkflowCatalog>();

			String wfId = "Wait1";
			await wfc.SaveAsync(new WorkflowDescriptor()
			{
				Id = wfId,
				Body = xaml,
				Format = "xaml"
			});
			var ident = await wfs.PublishAsync(wfc, wfId);

			var wfe = sp.GetService<IWorkflowEngine>();
			var inst = await wfe.CreateAsync(ident);
			inst = await wfe.RunAsync(inst.Id, new { X = 5 });
			var res = inst.Result;
			Assert.AreEqual(10, res.Get<Int32>("X"));

			inst = await wfe.ResumeAsync(inst.Id, "Activity_1rffhs5", new { V = 5 });
			res = inst.Result;
			Assert.AreEqual(15, res.Get<Int32>("X"));


		}

		[TestMethod]
		public async Task Exclusive()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\exclusive_gateway.bpmn");

			var sp = TestEngine.ServiceProvider();

			var wfs = sp.GetService<IWorkflowStorage>();
			var wfc = sp.GetService<IWorkflowCatalog>();

			String wfId = "Exclusive_1";
			await wfc.SaveAsync(new WorkflowDescriptor()
			{
				Id = wfId,
				Body = xaml,
				Format = "xaml"
			});
			var ident = await wfs.PublishAsync(wfc, wfId);

			var wfe = sp.GetService<IWorkflowEngine>();
			var inst = await wfe.CreateAsync(ident);
			inst = await wfe.RunAsync(inst.Id, new { X = 6 });
			var res = inst.Result;

			Assert.AreEqual("Yes", res.Get<String>("R"));

			inst = await wfe.CreateAsync(ident);
			inst = await wfe.RunAsync(inst.Id, new { X = 4 });
			res = inst.Result;
			Assert.AreEqual("No", res.Get<String>("R"));
		}

		[TestMethod]
		public async Task Counter()
		{
			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\UserTask_withCounter.bpmn");

			var sp = TestEngine.ServiceProvider();

			var wfs = sp.GetService<IWorkflowStorage>();
			var wfc = sp.GetService<IWorkflowCatalog>();

			String wfId = "Wait1WithCounter";
			await wfc.SaveAsync(new WorkflowDescriptor()
			{
				Id = wfId,
				Body = xaml,
				Format = "xaml"
			});
			var ident = await wfs.PublishAsync(wfc, wfId);

			var wfe = sp.GetService<IWorkflowEngine>();
			var inst = await wfe.CreateAsync(ident);
			inst = await wfe.RunAsync(inst.Id, new { X = 5 });
			var res = inst.Result;
			Assert.IsNull(res.Get<String>("R"));

			inst = await wfe.ResumeAsync(inst.Id, "CheckSaldo", new { Answer = 0 });
			res = inst.Result;
			Assert.AreEqual("1", res.Get<String>("R"));
		}
	}
}
