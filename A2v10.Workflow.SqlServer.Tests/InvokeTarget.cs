// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;
using System.Dynamic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;


using A2v10.Runtime.Interfaces;

namespace A2v10.Workflow.SqlServer.Tests
{
	[TestClass]
	[TestCategory("Storage.InvokeTarget")]
	public class InvokeTarget
	{
		private IServiceProvider _serviceProvider;

		[TestInitialize]
		public void Init()
		{
			_serviceProvider = TestEngine.ServiceProvider();
		}

		[TestMethod]
		public async Task StartWorkflow_Error()
		{
			var id = "DummyWorkflow";
			var target = _serviceProvider.GetService<IRuntimeInvokeTarget>();
			await Assert.ThrowsExceptionAsync<SqlServerStorageException>(() =>
			{
				return target.InvokeAsync("Start", new ExpandoObject()
				{
					{ "WorkflowId", id }
				});
			}, $"Workflow not found. (Id:'{id}', Version=0");
		}

		[TestMethod]
		public async Task CreateWorkflow_Error()
		{
			var id = "DummyWorkflow";
			var target = _serviceProvider.GetService<IRuntimeInvokeTarget>();
			await Assert.ThrowsExceptionAsync<SqlServerStorageException>(() =>
			{
				return target.InvokeAsync("Create", new ExpandoObject()
				{
					{ "WorkflowId", id }
				});
			}, $"Workflow not found. (Id:'{id}', Version=0");
		}

		[TestMethod]
		public async Task StartWorkflow_Success()
		{
			var id = "SimpleTarget";
			await TestEngine.PrepareDatabase(id);
			var target = _serviceProvider.GetService<IRuntimeInvokeTarget>();

		}
	}
}
