// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace A2v10.Workflow.Tests;
[TestClass]
[TestCategory("Bpmn.Events.Message")]
public class BpmnEventsMessage
{
    [TestMethod]
    public async Task BoundaryUninterrapted()
    {
        var boundaryId = "BoundaryMessage";
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\messages\\boundary.bpmn");
        var inst = await TestEngine.SimpleRun(boundaryId, xaml);
        Assert.IsNotNull(inst);
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.HasCount(2, log);
        Assert.AreEqual("startProcess|startSub", String.Join('|', log));

        var sp = TestEngine.ServiceProvider();
        var wfe = sp.GetRequiredService<IWorkflowEngine>();
        inst = await wfe.SendMessageAsync(inst.Id, "Message1");
        Assert.IsNotNull(inst);
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.HasCount(4, log);
        Assert.AreEqual("startProcess|startSub|messageBoundary|endBoundary", String.Join('|', log));

        inst = await wfe.ResumeAsync(inst.Id, "MainTask");
        Assert.IsNotNull(inst);
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.HasCount(6, log);
        Assert.AreEqual("startProcess|startSub|messageBoundary|endBoundary|endSub|endProcess", String.Join('|', log));
    }

    [TestMethod]
    public async Task BoundaryInterrapted()
    {
        var boundaryId = "BoundaryInterruptedMessage";
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\messages\\boundary_interrupted.bpmn");
        var inst = await TestEngine.SimpleRun(boundaryId, xaml);
        Assert.IsNotNull(inst);
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.HasCount(2, log);
        Assert.AreEqual("startProcess|startSub", String.Join('|', log));

        var sp = TestEngine.ServiceProvider();
        var wfe = sp.GetRequiredService<IWorkflowEngine>();
        inst = await wfe.SendMessageAsync(inst.Id, "Message1");
        Assert.IsNotNull(inst);
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.HasCount(5, log);
        Assert.AreEqual("startProcess|startSub|messageBoundary|endBoundary|endProcess", String.Join('|', log));
    }

    [TestMethod]
    public async Task MessageEnd()
    {
        var wfId = "MessageEnd";
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\messages\\endevent.bpmn");
        var inst = await TestEngine.SimpleRun(wfId, xaml);
        Assert.IsNotNull(inst);
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.HasCount(5, log);
        Assert.AreEqual("start|task1|endMessage|task2|end", String.Join('|', log));
    }
}

