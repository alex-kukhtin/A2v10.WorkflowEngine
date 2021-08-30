// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Threading.Tasks;
using System;

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
			await TestEngine.PrepareDatabase();
			var storage = _serviceProvider.GetService<IWorkflowStorage>();
			var catalog = _serviceProvider.GetService<IWorkflowCatalog>();
			var id = "Workflow1";
		}
	}
}
