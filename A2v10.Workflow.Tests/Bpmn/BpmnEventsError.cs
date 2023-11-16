// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace A2v10.Workflow.Tests;
[TestClass]
[TestCategory("Bpmn.Events.Error")]

public class BpmnErrorEvents
{
    [TestMethod]
    public async Task Simple()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\error\\error1.bpmn");

        String wfId = "Error1";
        var inst = await TestEngine.SimpleRun(wfId, xaml);

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        //Assert.AreEqual(6, log.Length);
        Assert.AreEqual("start|task1|endError|endBoundary", String.Join('|', log!));
    }

    [TestMethod]
    public async Task SubProcess()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\error\\error_sub.bpmn");

        String wfId = "Error1";
        var inst = await TestEngine.SimpleRun(wfId, xaml);

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        //Assert.AreEqual(6, log.Length);
        Assert.AreEqual("start|startSub|endErrorSub|endError", String.Join('|', log!));
    }


    [TestMethod]
    public async Task ParentChild()
    {
        String childId = "error_child";
        var xamlchild = File.ReadAllText("..\\..\\..\\TestFiles\\events\\error\\error_child.bpmn");
        var ident = await TestEngine.SimplePublish(childId, xamlchild);
        Assert.AreEqual(1, ident.Version);

        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\error\\error_parent.bpmn");

        String wfId = "ParentChild";
        var inst = await TestEngine.SimpleRun(wfId, xaml);

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        //Assert.AreEqual(6, log.Length);
        Assert.AreEqual("start|endError", String.Join('|', log!));
    }

    [TestMethod]
    public async Task ParentChildTimer()
    {
        String childId = "error_child_timer";
        var xamlchild = File.ReadAllText("..\\..\\..\\TestFiles\\events\\error\\error_child_timer.bpmn");
        var ident = await TestEngine.SimplePublish(childId, xamlchild);
        Assert.AreEqual(1, ident.Version);

        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\error\\error_parent_timer.bpmn");

        String wfId = "ParentChild";
        var inst = await TestEngine.SimpleRun(wfId, xaml);

        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(1, log!.Length);
        Assert.AreEqual("start", String.Join('|', log));

        await Task.Delay(1010);
        var wfe = TestEngine.ServiceProvider().GetRequiredService<IWorkflowEngine>();
        await wfe.ProcessPending();

        var inst2 = await wfe.LoadInstanceRaw(inst.Id);
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst2.ExecutionStatus);
        log = inst2.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(2, log!.Length);
        Assert.AreEqual("start|endError", String.Join('|', log));

    }
}

