// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.SqlServer.Tests;

[TestClass]
[TestCategory("Storage.CallActivity")]
public class CallActivity
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkflowStorage _workflowStorage;
    private readonly IWorkflowCatalog _workflowCatalog;
    private readonly IWorkflowEngine _workflowEngine;

    public CallActivity()
    {
        _serviceProvider = TestEngine.ServiceProvider();
        _workflowStorage = _serviceProvider.GetRequiredService<IWorkflowStorage>();
        _workflowCatalog = _serviceProvider.GetRequiredService<IWorkflowCatalog>();
        _workflowEngine = _serviceProvider.GetRequiredService<IWorkflowEngine>();
    }


    [TestMethod]
    public async Task SimpleWithTimer()
    {
        String parentId = "SimpleParent";
        String childId = "SimpleChild";

        await TestEngine.PrepareDatabase(childId);
        await TestEngine.PrepareDatabase(parentId);

        var xamlChild = File.ReadAllText("..\\..\\..\\TestFiles\\CallActivity\\SimpleChildTimer.bpmn");
        await _workflowCatalog.SaveAsync(new WorkflowDescriptor(childId, xamlChild));
        var childIdent = await _workflowStorage.PublishAsync(_workflowCatalog, childId);
        Assert.AreEqual(1, childIdent.Version);

        var xamlParent = File.ReadAllText("..\\..\\..\\TestFiles\\CallActivity\\SimpleParent.bpmn");
        await _workflowCatalog.SaveAsync(new WorkflowDescriptor(parentId, xamlParent));
        var parentIdent = await _workflowStorage.PublishAsync(_workflowCatalog, parentId);
        Assert.AreEqual(1, parentIdent.Version);

        var prms = new ExpandoObject()
        {
            {"A", 2 },
            {"B", 2 },
        };
        var inst = await _workflowEngine.CreateAsync(new WorkflowIdentity(parentId));
        inst = await _workflowEngine.RunAsync(inst.Id, prms);

        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        await Task.Delay(1010);
        await _workflowEngine.ProcessPending();

        var res = await _workflowEngine.LoadInstanceRaw(inst.Id);

        var res0 = res.Result;
        Assert.AreEqual(4, res0.Get<Double>("R"));
    }

    [TestMethod]
    public async Task CallCorrelationIdWithProcessKey()
    {
        String parentId = "CorrelationIdParent";
        String childId = "CorrelationIdChild";

        Int64 orderId = 224;

        await TestEngine.PrepareDatabase(childId);
        await TestEngine.PrepareDatabase(parentId);

        var xamlChild = File.ReadAllText("..\\..\\..\\TestFiles\\CallActivity\\CorrelationIdChild.bpmn");
        await _workflowCatalog.SaveAsync(new WorkflowDescriptor(childId, xamlChild) { Key = "CorrelationChildKey" });
        var childIdent = await _workflowStorage.PublishAsync(_workflowCatalog, childId);
        Assert.AreEqual(1, childIdent.Version);

        var xamlParent = File.ReadAllText("..\\..\\..\\TestFiles\\CallActivity\\CorrelationIdParent.bpmn");
        await _workflowCatalog.SaveAsync(new WorkflowDescriptor(parentId, xamlParent));
        var parentIdent = await _workflowStorage.PublishAsync(_workflowCatalog, parentId);
        Assert.AreEqual(1, parentIdent.Version);

        var inst = await _workflowEngine.CreateAsync(new WorkflowIdentity(parentId), orderId.ToString());
        inst = await _workflowEngine.RunAsync(inst.Id);

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);

        var res0 = inst.Result;
        Assert.IsNotNull(res0);
        Assert.AreEqual(6, res0.Get<Int32>("Result"));
        Assert.AreEqual("FromChild", res0.Eval<String>("Order.State"));
        Assert.AreEqual(6, res0.Eval<Int32>("Order.Count"));
        Assert.AreEqual(orderId, res0.Eval<Int64>("Order.Id"));

    }

    [TestMethod]
    public async Task CallCorrelationIdWithProcessKeyGuid()
    {
        String parentId = "CorrelationIdParent2";
        String childId = "4A33375A-6129-4BA7-8904-EC36085ECACB";

        Int64 orderId = 291;

        await TestEngine.PrepareDatabase(childId);
        await TestEngine.PrepareDatabase(parentId);

        var xamlChild = File.ReadAllText("..\\..\\..\\TestFiles\\CallActivity\\CorrelationIdChild.bpmn");
        await _workflowCatalog.SaveAsync(new WorkflowDescriptor(childId, xamlChild) { Key = "CorrelationChildKey" });
        var childIdent = await _workflowStorage.PublishAsync(_workflowCatalog, childId);
        Assert.AreEqual(1, childIdent.Version);

        var xamlParent = File.ReadAllText("..\\..\\..\\TestFiles\\CallActivity\\CorrelationIdParent.bpmn");
        await _workflowCatalog.SaveAsync(new WorkflowDescriptor(parentId, xamlParent));
        var parentIdent = await _workflowStorage.PublishAsync(_workflowCatalog, parentId);
        Assert.AreEqual(1, parentIdent.Version);

        var inst = await _workflowEngine.CreateAsync(new WorkflowIdentity(parentId), orderId.ToString());
        inst = await _workflowEngine.RunAsync(inst.Id);

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);

        var res0 = inst.Result;
        Assert.IsNotNull(res0);
        Assert.AreEqual(6, res0.Get<Int32>("Result"));
        Assert.AreEqual("FromChild", res0.Eval<String>("Order.State"));
        Assert.AreEqual(6, res0.Eval<Int32>("Order.Count"));
        Assert.AreEqual(orderId, res0.Eval<Int64>("Order.Id"));

    }


    [TestMethod]
    public async Task CallCorrelationIdCollection()
    {
        String parentId = "CorrCollectionParent";
        String childId = "CorrCollectionChild";

        Int64 orderId = 291;

        await TestEngine.PrepareDatabase(childId);
        await TestEngine.PrepareDatabase(parentId);

        var xamlChild = File.ReadAllText("..\\..\\..\\TestFiles\\CallActivity\\CorrIdCollChild.bpmn");
        await _workflowCatalog.SaveAsync(new WorkflowDescriptor(childId, xamlChild));
        var childIdent = await _workflowStorage.PublishAsync(_workflowCatalog, childId);
        Assert.AreEqual(1, childIdent.Version);

        var xamlParent = File.ReadAllText("..\\..\\..\\TestFiles\\CallActivity\\CorrIdCollParent.bpmn");
        await _workflowCatalog.SaveAsync(new WorkflowDescriptor(parentId, xamlParent));
        var parentIdent = await _workflowStorage.PublishAsync(_workflowCatalog, parentId);
        Assert.AreEqual(1, parentIdent.Version);

        var inst = await _workflowEngine.CreateAsync(new WorkflowIdentity(parentId), orderId.ToString());
        inst = await _workflowEngine.RunAsync(inst.Id);

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);

        var res0 = inst.Result;
        Assert.IsNotNull(res0);
        Assert.AreEqual(orderId * 3 + 6 + 3, res0.Get<Int32>("Result"));

    }
}

