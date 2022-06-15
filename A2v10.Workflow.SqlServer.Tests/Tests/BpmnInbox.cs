// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

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
        inst = await engine.ResumeAsync(inst.Id, "Inbox");

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
    }
}

