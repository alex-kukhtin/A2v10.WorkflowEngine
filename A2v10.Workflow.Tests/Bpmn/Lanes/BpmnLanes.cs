// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace A2v10.Workflow.Tests;

[TestClass]
[TestCategory("Bpmn.Lanes")]
public class BpmnLanes
{
    [TestMethod]
    public async Task SimpleLane()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\lanes\\simple.bpmn");

        String wfId = "SimpleLane";
        var inst = await TestEngine.SimpleRun(wfId, xaml);

        var res0 = inst.Result;
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        var log = res0.Get<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(3, log.Length);
        Assert.AreEqual("start|task|end", String.Join('|', log));
    }
}
