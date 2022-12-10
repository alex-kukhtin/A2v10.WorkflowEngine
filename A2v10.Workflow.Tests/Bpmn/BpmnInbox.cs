// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace A2v10.Workflow.Tests;

[TestClass]
[TestCategory("Bpmn.Inbox")]
public class BpmnInbox
{
    [TestMethod]
    public async Task SimpleInbox()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\inbox\\inbox_1.bpmn");

        String wfId = "InboxSimple";

        var inst = await TestEngine.SimpleRun(wfId, xaml);
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        var sp = TestEngine.ServiceProvider();
        var engine = sp.GetRequiredService<IWorkflowEngine>();
        inst = await engine.ResumeAsync(inst.Id, "Inbox");

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
    }

    [TestMethod]
    public async Task InboxVariableBookmark()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\inbox\\inbox_variable.bpmn");

        String wfId = "InboxVariableBookmark";

        var inst = await TestEngine.SimpleRun(wfId, xaml);
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(1, log!.Length);
        Assert.AreEqual("start", String.Join('|', log));

        var sp = TestEngine.ServiceProvider();
        var engine = sp.GetRequiredService<IWorkflowEngine>();
        inst = await engine.ResumeAsync(inst.Id, "BookmarkName_value");
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(3, log!.Length);
        Assert.AreEqual("start|inbox:BookmarkName|end", String.Join('|', log));

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
    }

    [TestMethod]
    public async Task InboxBoundary()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\inbox\\inbox_boundary.bpmn");

        String wfId = "InboxBoundary";

        var inst = await TestEngine.SimpleRun(wfId, xaml);
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
        Assert.AreEqual(1, inst.InstanceData?.Inboxes?.InboxCreate.Count);

        var sp = TestEngine.ServiceProvider();
        var engine = sp.GetRequiredService<IWorkflowEngine>();

        await Task.Delay(1001);
        await engine.ProcessPending();

        inst = await engine.LoadInstanceRaw(inst.Id);

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(2, log!.Length);
        Assert.AreEqual("start|endTimer", String.Join('|', log));
        Assert.IsNull(inst.InstanceData?.Inboxes);
    }
}

