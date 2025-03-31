// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Tests;

[TestClass]
[TestCategory("Bpmn.Loops")]
public class BpmnLoops
{
    [TestMethod]
    public async Task SimpleLoop()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\loop\\loop_simple.bpmn");

        String wfId = "SimpleLoop";
        var inst = await TestEngine.SimpleRun(wfId, xaml);

        var res0 = inst.Result;
        Assert.AreEqual(10, res0.Get<Double>("X"));
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
    }

    [TestMethod]
    public async Task LoopMaximum()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\loop\\loop_maximum.bpmn");

        String wfId = "SimpleLoop";
        var inst = await TestEngine.SimpleRun(wfId, xaml);

        var res0 = inst.Result;
        Assert.AreEqual(12, res0.Get<Double>("X"));
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
    }

    [TestMethod]
    public async Task LoopTestBefore()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\loop\\loop_testbefore.bpmn");

        String wfId = "SimpleLoop";
        var inst = await TestEngine.SimpleRun(wfId, xaml);

        var res0 = inst.Result;
        Assert.AreEqual(10, res0.Get<Double>("X"));
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
    }

    [TestMethod]
    public async Task LoopTestNotBefore()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\loop\\loop_testnotbefore.bpmn");

        String wfId = "SimpleLoop";
        var inst = await TestEngine.SimpleRun(wfId, xaml);

        var res0 = inst.Result;
        Assert.AreEqual(9, res0.Get<Double>("X"));
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
    }

    [TestMethod]
    public async Task LoopWithParallel()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\loop\\loop_withParallel.bpmn");

        String wfId = "LoopWithParallel";
        var inst = await TestEngine.SimpleRun(wfId, xaml);

        var res0 = inst.Result;
        Assert.IsNull(res0.Get<String>("res"));

        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        var eng = TestEngine.ServiceProvider().GetRequiredService<IWorkflowEngine>();
        var reply = new ExpandoObject()
        {
            { "Answer", 0 }
        };

        var inst2 = await eng.ResumeAsync(inst.Id, "EnterSaldo", reply);
        var res2 = inst2.Result;
        Assert.IsNotNull(res2);

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst2.ExecutionStatus);
    }

    [TestMethod]
    public async Task LoopSubSub()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\loop\\loop_sub_sub.bpmn");

        String wfId = "LoopSubSub";
        var inst = await TestEngine.SimpleRun(wfId, xaml);

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(12, log!.Length);
        Assert.AreEqual("start|startSub|startSubSub|count:1|endSubSub|endSub|startSub|startSubSub|count:0|endSubSub|endSub|end", String.Join('|', log));

    }

    [TestMethod]
    public async Task LoopSubSubError()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\loop\\loop_sub_sub_error.bpmn");

        String wfId = "LoopSubSubError";
        var inst = await TestEngine.SimpleRun(wfId, xaml);

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(17, log!.Length);
        Assert.AreEqual("start|startSub|startSubSub|count:2|endSubSubError|endSubError|startSub|startSubSub|count:1|endSubSub|endSub|startSub|startSubSub|count:0|endSubSub|endSub|end", String.Join('|', log));
    }

    [TestMethod]
    public async Task LoopSubSubErrorWithoutError()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\loop\\loop_sub_sub_error2.bpmn");

        String wfId = "LoopSubSubError";
        var inst = await TestEngine.SimpleRun(wfId, xaml);

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(12, log!.Length);
        Assert.AreEqual("start|startSub|startSubSub|count:1|endSubSub|endSub|startSub|startSubSub|count:0|endSubSub|endSub|end", String.Join('|', log));
    }
}