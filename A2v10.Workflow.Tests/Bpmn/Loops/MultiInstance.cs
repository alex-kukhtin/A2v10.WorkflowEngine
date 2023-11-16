// Copyright © 2020-2022 Oleksandr Kukhtin. All rights reserved.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Tests;

[TestClass]
[TestCategory("Bpmn.MultiInstance")]
public class BpmnMultiInstance
{
    [TestMethod]
    public async Task SimpleMI()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\multiinstance\\mi_simple.bpmn");

        String wfId = "SimpleMI";
        var inst = await TestEngine.SimpleRun(wfId, xaml);

        var res0 = inst.Result;
        Assert.AreEqual(41, res0.Get<Double>("X"));
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
    }

    [TestMethod]
    public async Task SimpleMISequential()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\multiinstance\\mi_simple_seq.bpmn");

        String wfId = "SimpleMI";
        var inst = await TestEngine.SimpleRun(wfId, xaml);

        var res0 = inst.Result;
        Assert.AreEqual(35, res0.Get<Double>("X")); /* use index */
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
    }
}