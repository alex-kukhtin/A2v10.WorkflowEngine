// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace A2v10.Workflow.SqlServer.Tests
{
    [TestClass]
    [TestCategory("Storage.Workflow")]
    public class WorkflowStorage
    {
        private readonly IServiceProvider _serviceProvider;


        public WorkflowStorage()
        {
            _serviceProvider = TestEngine.ServiceProvider();
        }

        [TestInitialize]
        public void Init()
        {
        }

        [TestMethod]
        public async Task Publish()
        {
            var id = "Publish_Workflow1";
            await TestEngine.PrepareDatabase(id);
            var storage = _serviceProvider.GetRequiredService<IWorkflowStorage>();
            var catalog = _serviceProvider.GetRequiredService<IWorkflowCatalog>();
            await catalog.SaveAsync(new WorkflowDescriptor(id, "<test></test>", "text/xml"));
            var inst = await storage.PublishAsync(catalog, id);
            Assert.AreEqual(1, inst.Version);
            Assert.AreEqual(id, inst.Id);
        }

        [TestMethod]
        public async Task PublishVersions()
        {
            var id = "Publish_Workflow2";
            await TestEngine.PrepareDatabase(id);
            var storage = _serviceProvider.GetRequiredService<IWorkflowStorage>();
            var catalog = _serviceProvider.GetRequiredService<IWorkflowCatalog>();
            var text1 = "<test></test>";
            var format = "text/xml";
            var text2 = "<test><inner/></test>";

            await catalog.SaveAsync(new WorkflowDescriptor(Id: id, Body: text1, Format: format));
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

            await catalog.SaveAsync(new WorkflowDescriptor(Id: id, Body: text1, Format: format));
            {
                var inst3 = await storage.PublishAsync(catalog, id);
                Assert.AreEqual(1, inst3.Version);
                Assert.AreEqual(id, inst3.Id);
            }

            await catalog.SaveAsync(new WorkflowDescriptor(Id: id, Body: text2, Format: format));
            {
                var inst4 = await storage.PublishAsync(catalog, id);
                Assert.AreEqual(2, inst4.Version);
                Assert.AreEqual(id, inst4.Id);
            }
        }
    }
}
