// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Workflow.Interfaces;
using System.Dynamic;

namespace A2v10.Workflow.SqlServer.Tests;

[TestClass]
[TestCategory("Bpmn.Inbox")]
public class BpmnInbox
{
    [TestMethod]
    public async Task SimpleInbox()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\inbox\\inbox_1.bpmn");

        String wfId = "InboxSimple";

        await TestEngine.PrepareDatabase(wfId);

        var inst = await TestEngine.SimpleRun(wfId, xaml);
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        var sp = TestEngine.ServiceProvider();
        var engine = sp.GetRequiredService<IWorkflowEngine>();
        inst = await engine.ResumeAsync(inst.Id, "Inbox", new ExpandoObject()
        {
            { "Answer", "OK" }
        });


        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);

        var lr = inst.State.Get<ExpandoObject>("LastResult");
        Assert.AreEqual("OK", lr?.Get<String>("Answer"));
    }

    [TestMethod]
    public async Task TwoInboxes()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\inbox\\two_Inboxes.bpmn");

        String wfId = "TwoInboxes";

        await TestEngine.PrepareDatabase(wfId);

        var inst = await TestEngine.SimpleRun(wfId, xaml);
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        var sp = TestEngine.ServiceProvider();
        var engine = sp.GetRequiredService<IWorkflowEngine>();

        inst = await engine.ResumeAsync(inst.Id, "Task_1", new ExpandoObject()
        {
            { "Answer", "Cancel" }
        });
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
        var lr = inst.State.Get<ExpandoObject>("LastResult");
        Assert.AreEqual("Cancel", lr?.Get<String>("Answer"));

        inst = await engine.ResumeAsync(inst.Id, "Task_2", new ExpandoObject()
        {
            { "Answer", "OK" }
        });
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        inst = await engine.ResumeAsync(inst.Id, "Task_1", new ExpandoObject()
        {
            { "Answer", "OK" }
        });

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);

        lr = inst.State.Get<ExpandoObject>("LastResult");
        Assert.AreEqual("OK", lr?.Get<String>("Answer"));
    }


    [TestMethod]
    public async Task ParallelInboxes()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\inbox\\parallel_inboxes.bpmn");

        String wfId = "ParallelInboxes";

        await TestEngine.PrepareDatabase(wfId);

        var inst = await TestEngine.SimpleRun(wfId, xaml);
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        var sp = TestEngine.ServiceProvider();
        var engine = sp.GetRequiredService<IWorkflowEngine>();

        inst = await engine.ResumeAsync(inst.Id, "Task_1", new ExpandoObject()
        {
            { "Answer", "Cancel" }
        });
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
        var lr = inst.State.Get<ExpandoObject>("LastResult");
        Assert.AreEqual("Cancel", lr?.Get<String>("Answer"));

        inst = await engine.ResumeAsync(inst.Id, "Task_2", new ExpandoObject()
        {
            { "Answer", "OK" }
        });
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        await Task.Delay(1020); // 1
        await engine.ProcessPending();

        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        lr = inst.State.Get<ExpandoObject>("LastResult");
        Assert.AreEqual("OK", lr?.Get<String>("Answer"));
    }
}

