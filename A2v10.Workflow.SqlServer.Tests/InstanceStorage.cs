// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;


using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.SqlServer.Tests
{
	[TestClass]
	[TestCategory("Storage.Instance")]
	public class InstanceStorage
	{
		private IServiceProvider _serviceProvider;

		[TestInitialize]
		public void Init()
		{
			_serviceProvider = TestEngine.ServiceProvider();
		}

		[TestMethod]
		public async Task SimpleInstance()
		{
			var id = "Simple_Instance_1";
			await TestEngine.PrepareDatabase(id);

			var storage = _serviceProvider.GetService<IWorkflowStorage>();
			var catalog = _serviceProvider.GetService<IWorkflowCatalog>();
			var engine = _serviceProvider.GetService<IWorkflowEngine>();


			var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\simple.bpmn");
			var format = "text/xml";

			await catalog.SaveAsync(new WorkflowDescriptor()
			{
				Id = id,
				Body = xaml,
				Format = format
			});

			var ident = await storage.PublishAsync(catalog, id);

			Assert.AreEqual(1, ident.Version);

			var inst = await engine.CreateAsync(ident);

			inst = await engine.RunAsync(inst);
		}
	}
}
