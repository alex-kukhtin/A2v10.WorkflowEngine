// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;


using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.SqlServer.Tests
{
	[TestClass]
	[TestCategory("Storage.Workflow")]
	public class WorkflowStorage
	{
		private IServiceProvider _serviceProvider;

		[TestInitialize]
		public void Init()
		{
			_serviceProvider = TestEngine.ServiceProvider();
		}

		[TestMethod]
		public async Task Publish()
		{
			await TestEngine.PrepareDatabase();
			var storage = _serviceProvider.GetService<IWorkflowStorage>();
			var catalog = _serviceProvider.GetService<IWorkflowCatalog>();
			var id = "Workflow1";
			await catalog.SaveAsync(new WorkflowDescriptor()
			{
				Id = id,
				Body = "<test></test>",
				Format = "text/xml"
			});
			var inst = await storage.PublishAsync(catalog, id);
			Assert.AreEqual(1, inst.Version);
			Assert.AreEqual(id, inst.Id);
		}

		[TestMethod]
		public async Task PublishVersions()
		{
			await TestEngine.PrepareDatabase();
			var storage = _serviceProvider.GetService<IWorkflowStorage>();
			var catalog = _serviceProvider.GetService<IWorkflowCatalog>();
			var id = "Workflow1";
			var text1 = "<test></test>";
			var format = "text/xml";
			var text2 = "<test><inner/></test>";

			await catalog.SaveAsync(new WorkflowDescriptor()
			{
				Id = id,
				Body = text1,
				Format = format
			});
			{
				var inst = await storage.PublishAsync(catalog, id);
				Assert.AreEqual(1, inst.Version);
				Assert.AreEqual(id, inst.Id);
			}

			{
				var inst2 = await storage.PublishAsync(catalog, id);
				Assert.AreEqual(1, inst2.Version);
				Assert.AreEqual(id, inst2.Id);
			}

			await catalog.SaveAsync(new WorkflowDescriptor()
			{
				Id = id,
				Body = text1,
				Format = format
			});
			{
				var inst3 = await storage.PublishAsync(catalog, id);
				Assert.AreEqual(1, inst3.Version);
				Assert.AreEqual(id, inst3.Id);
			}

			await catalog.SaveAsync(new WorkflowDescriptor()
			{
				Id = id,
				Body = text2,
				Format = format
			});
			{
				var inst4 = await storage.PublishAsync(catalog, id);
				Assert.AreEqual(2, inst4.Version);
				Assert.AreEqual(id, inst4.Id);
			}
		}
	}
}
