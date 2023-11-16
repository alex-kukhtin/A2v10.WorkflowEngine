// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace A2v10.Workflow.SqlServer.Tests;

[TestClass]
[TestCategory("Storage.Instance")]
public class InstanceStorage
{
    private readonly IServiceProvider _serviceProvider;

    public InstanceStorage()
    {
        _serviceProvider = TestEngine.ServiceProvider();
    }

    [TestInitialize]
    public void Init()
    {
    }

    [TestMethod]
    public void CheckVersion()
		{
        var sv = _serviceProvider.GetRequiredService<IWorkflowStorageVersion>();
        var version = sv.GetVersion();
        Assert.IsTrue(version.Valid);
        Assert.AreEqual(version.Required, version.Actual);
    }

    [TestMethod]
    public async Task SimpleInstance()
    {
        var id = "Simple_Instance_1";
        await TestEngine.PrepareDatabase(id);

        var storage = _serviceProvider.GetRequiredService<IWorkflowStorage>();
        var catalog = _serviceProvider.GetRequiredService<IWorkflowCatalog>();
        var engine = _serviceProvider.GetRequiredService<IWorkflowEngine>();


        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\simple.bpmn");
        var format = "text/xml";

        await catalog.SaveAsync(new WorkflowDescriptor(id, xaml, format));

        var ident = await storage.PublishAsync(catalog, id);

        Assert.AreEqual(1, ident.Version);

        var inst = await engine.CreateAsync(ident);

        inst = await engine.RunAsync(inst);

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
    }
}
