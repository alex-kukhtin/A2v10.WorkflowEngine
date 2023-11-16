// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using A2v10.Runtime.Interfaces;
using A2v10.Workflow.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;

namespace A2v10.Workflow.SqlServer.Tests
{
    [TestClass]
    [TestCategory("Storage.InvokeTarget")]
    public class InvokeTarget
    {
        private readonly IServiceProvider _serviceProvider;

        public InvokeTarget()
        {
            _serviceProvider = TestEngine.ServiceProvider();

        }

        [TestInitialize]
        public void Init()
        {
        }

        [TestMethod]
        public async Task StartWorkflow_Error()
        {
            var id = "DummyWorkflow";
            var target = _serviceProvider.GetRequiredService<IRuntimeInvokeTarget>();
            var ex = await Assert.ThrowsExceptionAsync<SqlServerStorageException>(() =>
            {
                return target.InvokeAsync("Start", new ExpandoObject()
                {
                    { "WorkflowId", id }
                });
            });
            Assert.AreEqual($"Workflow not found. (Id:'{id}', Version:0)", ex.Message);
        }

        [TestMethod]
        public async Task RunWorkflow_Error()
        {
            var target = _serviceProvider.GetRequiredService<IRuntimeInvokeTarget>();
            var ex = await Assert.ThrowsExceptionAsync<WorkflowException>(() =>
            {
                return target.InvokeAsync("Run", new ExpandoObject()
                {
                    { "x", "5" }
                });
            });
            Assert.AreEqual("Run. InstanceId is required", ex.Message);
        }

        [TestMethod]
        public async Task CreateWorkflow_Error()
        {
            var id = "DummyWorkflow";
            var target = _serviceProvider.GetRequiredService<IRuntimeInvokeTarget>();
            var ex = await Assert.ThrowsExceptionAsync<SqlServerStorageException>(() =>
            {
                return target.InvokeAsync("Create", new ExpandoObject()
                {
                    { "WorkflowId", id }
                });
            });
            Assert.AreEqual($"Workflow not found. (Id:'{id}', Version:0)", ex.Message);
        }

        [TestMethod]
        public async Task StartWorkflow_Success()
        {
            var id = "SimpleTarget";
            await TestEngine.PrepareDatabase(id);

            var target = _serviceProvider.GetRequiredService<IRuntimeInvokeTarget>();

            var format = "xaml";
            var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\simple.bpmn");

            await target.InvokeAsync("Save", new ExpandoObject()
            {
                { "WorkflowId", id },
                { "Format", format },
                { "Body", xaml }
            });

            await target.InvokeAsync("Publish", new ExpandoObject()
            {
                {"WorkflowId", id }
            });

            var res = await target.InvokeAsync("Start", new ExpandoObject()
            {
                {"WorkflowId", id },
                {"Args", new ExpandoObject()
                    {
                        {"X", 5 }
                    }
                }
            });

            Assert.AreEqual(10.0, res.Eval<Double>("Result.X"));
        }
    }
}
