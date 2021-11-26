// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;
using System.Dynamic;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using A2v10.Workflow.Interfaces;
using A2v10.Data;
using A2v10.Data.Interfaces;

namespace A2v10.Workflow.SqlServer.Tests
{
	[TestClass]
	[TestCategory("Bpmn.Full")]
	public class FullBmpn
	{
		private readonly IServiceProvider _serviceProvider;

		public FullBmpn() {
			_serviceProvider = TestEngine.ServiceProvider();
		}

		[TestInitialize]
		public void Init()
		{
		}

		[TestMethod]
		public async Task LoopWithTimer()
		{
			var id = "LoopTimer_Instance_1";
			await TestEngine.PrepareDatabase(id);

			var storage = _serviceProvider.GetRequiredService<IWorkflowStorage>();
			var catalog = _serviceProvider.GetRequiredService<IWorkflowCatalog>();
			var engine = _serviceProvider.GetRequiredService<IWorkflowEngine>();
			var dbContext = _serviceProvider.GetRequiredService<IDbContext>();


			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\loop_with_timers.bpmn");
			var format = "text/xml";

			await catalog.SaveAsync(new WorkflowDescriptor(id, xaml, format));

			var ident = await storage.PublishAsync(catalog, id);

			Assert.AreEqual(1, ident.Version);

			var inst = await engine.CreateAsync(ident);
			inst = await engine.RunAsync(inst);

			Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
			var res = inst.Result;
			Assert.AreEqual("1", res.Get<String>("res"));

			Thread.Sleep(1100);
			await engine.ProcessPending();

			var mdPrms = new ExpandoObject()
			{
				{"Id", inst.Id }
			};

			async Task AssertModel(String execState, String resVal)
			{
				var instModel = await dbContext.LoadModelAsync(null, "a2wf_test.[Instance.Load.Unlocked]", mdPrms);
				Assert.AreEqual(execState, instModel.Eval<String>("Instance.ExecutionStatus"));
				String? state = instModel.Eval<String>("Instance.State");
				Assert.IsNotNull(state);
				var stateObj = JsonConvert.DeserializeObject<ExpandoObject>(state);
				Assert.IsNotNull(stateObj);
				String? strVal = stateObj.Eval<String>("Variables.Process_1.res");
				Assert.AreEqual(resVal, strVal);
			}

			await AssertModel("Idle", "2");

			Thread.Sleep(1100);
			await engine.ProcessPending();

			await AssertModel("Idle", "3");

			Thread.Sleep(1100);
			await engine.ProcessPending();
			await AssertModel("Complete", "3end");

		}


		[TestMethod]
		public async Task DateTimerFromDb()
		{
			var id = "DateTimer1";
			await TestEngine.PrepareDatabase(id);

			var storage = _serviceProvider.GetRequiredService<IWorkflowStorage>();
			var catalog = _serviceProvider.GetRequiredService<IWorkflowCatalog>();
			var engine = _serviceProvider.GetRequiredService<IWorkflowEngine>();
			var dbContext = _serviceProvider.GetRequiredService<IDbContext>();


			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\timerfromdb.bpmn");
			var format = "text/xml";

			await catalog.SaveAsync(new WorkflowDescriptor(id, xaml, format));

			var ident = await storage.PublishAsync(catalog, id);

			Assert.AreEqual(1, ident.Version);

			var inst = await engine.CreateAsync(ident);
			inst = await engine.RunAsync(inst);

			Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
			var res = inst.Result;
			Assert.AreEqual("Start", res.Get<String>("Result"));

			Thread.Sleep(1100);
			await engine.ProcessPending();

			var mdPrms = new ExpandoObject()
			{
				{"Id", inst.Id }
			};

			var instModel = await dbContext.LoadModelAsync(null, "a2wf_test.[Instance.Load.Unlocked]", mdPrms);
			Assert.AreEqual("Complete", instModel.Eval<String>("Instance.ExecutionStatus"));
			String? state = instModel.Eval<String>("Instance.State");
			Assert.IsNotNull(state);
			var stateObj = JsonConvert.DeserializeObject<ExpandoObject>(state);
			Assert.IsNotNull(stateObj);
			String? strVal = stateObj.Eval<String>("Variables.Process_1.Result");
			Assert.AreEqual("StartEnd", strVal);
		}
	}
}
