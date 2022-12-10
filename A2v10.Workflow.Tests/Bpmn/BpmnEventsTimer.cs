// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace A2v10.Workflow.Tests;

[TestClass]
[TestCategory("Bpmn.Events.Timer")]
public class BpmnTimerEvents
{
    [TestMethod]
    public async Task BoundaryTimer()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\boundary_simple.bpmn");

        var sp = TestEngine.ServiceProvider();

        var wfs = sp.GetRequiredService<IWorkflowStorage>();
        var wfc = sp.GetRequiredService<IWorkflowCatalog>();

        String wfId = "BoundarySimple";
        await wfc.SaveAsync(new WorkflowDescriptor(Id: wfId, Body: xaml, Format: "xaml"));
        var ident = await wfs.PublishAsync(wfc, wfId);

        var wfe = sp.GetRequiredService<IWorkflowEngine>();
        var inst = await wfe.CreateAsync(ident);
        inst = await wfe.RunAsync(inst);
        var res0 = inst.Result;
        Assert.IsNull(res0.Get<String>("Result"));
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        var evList = new List<String>() { "Event1" };
        var instTimer = await wfe.HandleEventsAsync(inst.Id, evList);
        var res1 = instTimer.Result;
        Assert.AreEqual("Timer", res1.Get<String>("Result"));
        Assert.AreEqual(WorkflowExecutionStatus.Idle, instTimer.ExecutionStatus);

        var instResume = await wfe.ResumeAsync(inst.Id, "UserTask1");
        var res2 = instResume.Result;
        Assert.AreEqual("Normal", res2.Get<String>("Result"));
        Assert.AreEqual(WorkflowExecutionStatus.Complete, instResume.ExecutionStatus);
    }

    [TestMethod]
    public async Task IntermediateTimer()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\timer\\intermediate_timer.bpmn");

        var sp = TestEngine.ServiceProvider();

        var wfs = sp.GetRequiredService<IWorkflowStorage>();
        var wfc = sp.GetRequiredService<IWorkflowCatalog>();
        var ins = sp.GetRequiredService<IInstanceStorage>();

        String wfId = "IntermediateSimple";
        await wfc.SaveAsync(new WorkflowDescriptor(wfId, xaml));
        var ident = await wfs.PublishAsync(wfc, wfId);

        var wfe = sp.GetRequiredService<IWorkflowEngine>();
        var inst = await wfe.CreateAsync(ident);
        inst = await wfe.RunAsync(inst);
        var res0 = inst.Result;
        Assert.AreEqual("Start", res0.Get<String>("Result"));
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        await Task.Delay(1001);
        await wfe.ProcessPending();
        var instAfter = await ins.Load(inst.Id);
        var res1 = instAfter.Result;
        Assert.AreEqual("AfterTimer", res1.Get<String>("Result"));
        Assert.AreEqual(WorkflowExecutionStatus.Complete, instAfter.ExecutionStatus);
    }

    [TestMethod]
    public async Task IntermediateTimerMultiply()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\timer\\intermediate_timer_2.bpmn");

        String wfId = "IntermediateMult";

        var inst = await TestEngine.SimpleRun(wfId, xaml);

        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(1, log!.Length);
        Assert.AreEqual("start", String.Join('|', log));

        var sp = TestEngine.ServiceProvider();
        var wfe = sp.GetRequiredService<IWorkflowEngine>();
        var ins = sp.GetRequiredService<IInstanceStorage>();

        await Task.Delay(1010);
        await wfe.ProcessPending();

        inst = await ins.Load(inst.Id);
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(3, log!.Length);
        Assert.AreEqual("start|timer|endTimer", String.Join('|', log));

        await Task.Delay(1010);
        await wfe.ProcessPending();
        inst = await ins.Load(inst.Id);
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(5, log!.Length);
        Assert.AreEqual("start|timer|endTimer|timer|endTimer", String.Join('|', log));

        inst = await wfe.ResumeAsync(inst.Id, "Bookmark");
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(7, log!.Length);
        Assert.AreEqual("start|timer|endTimer|timer|endTimer|userTask|endUser", String.Join('|', log));
    }

    [TestMethod]
    public async Task VariableTimer()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\timer\\variable_timer.bpmn");

        String wfId = "VariableTimer";
        var prms = new ExpandoObject()
        {
            { "Interval", "00:00:01" }
        };

        var wfe = TestEngine.ServiceProvider().GetRequiredService<IWorkflowEngine>();
        var ins = TestEngine.ServiceProvider().GetRequiredService<IInstanceStorage>();

        var inst = await TestEngine.SimpleRun(wfId, xaml, prms);

        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        Thread.Sleep(1100);
        await wfe.ProcessPending();

        var instAfter = await ins.Load(inst.Id);
        var res1 = instAfter.Result;
        Assert.AreEqual("AfterTimer", res1.Get<String>("Result"));
        Assert.AreEqual(WorkflowExecutionStatus.Complete, instAfter.ExecutionStatus);
    }

    [TestMethod]
    public async Task IntermediageTimerDate()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\timer\\intermediate_timer_date.bpmn");

        String wfId = "Intermediate";

        var now = DateTime.UtcNow + TimeSpan.FromSeconds(1);
        var prms = new ExpandoObject()
        {
            { "Time", now }
        };

        var wfe = TestEngine.ServiceProvider().GetRequiredService<IWorkflowEngine>();
        var ins = TestEngine.ServiceProvider().GetRequiredService<IInstanceStorage>();

        var inst = await TestEngine.SimpleRun(wfId, xaml, prms);

        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        Thread.Sleep(1100);
        await wfe.ProcessPending();

        var instAfter = await ins.Load(inst.Id);
        var res1 = instAfter.Result;
        Assert.AreEqual("AfterTimer", res1.Get<String>("Result"));
        Assert.AreEqual(WorkflowExecutionStatus.Complete, instAfter.ExecutionStatus);
    }


    [TestMethod]
    public async Task IntermediageTimerBookmarks()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\timer\\intermediate_timer_with_bookmarks.bpmn");

        String wfId = "IntermediateMultWithBookmark";

        var wfe = TestEngine.ServiceProvider().GetRequiredService<IWorkflowEngine>();
        var ist = TestEngine.ServiceProvider().GetRequiredService<IInstanceStorage>();

        var inst = await TestEngine.SimpleRun(wfId, xaml);
        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(1, log!.Length);
        Assert.AreEqual("start", String.Join('|', log));

        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        await Task.Delay(1100); // 1
        await wfe.ProcessPending();
        inst = await ist.Load(inst.Id);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(2, log!.Length);
        Assert.AreEqual("start|timerNI", String.Join('|', log));

        await Task.Delay(1100); // 2
        await wfe.ProcessPending();
        inst = await ist.Load(inst.Id);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(3, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI", String.Join('|', log));

        await Task.Delay(1100); // 3
        await wfe.ProcessPending();
        inst = await ist.Load(inst.Id);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(5, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI|timerNI|timerI", String.Join('|', log));

        inst = await wfe.ResumeAsync(inst.Id, "BookMark2", new ExpandoObject()
        {
            {"Answer", "CONTINUE"}
        });
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(6, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI|timerNI|timerI|Bookmark2:CONTINUE", String.Join('|', log));
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        await Task.Delay(1100); // 1
        await wfe.ProcessPending();
        inst = await ist.Load(inst.Id);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(7, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI|timerNI|timerI|Bookmark2:CONTINUE|timerNI", String.Join('|', log));

        await Task.Delay(1100); // 2
        await wfe.ProcessPending();
        inst = await ist.Load(inst.Id);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(8, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI|timerNI|timerI|Bookmark2:CONTINUE|timerNI|timerNI", String.Join('|', log));

        await Task.Delay(1100); // 3
        await wfe.ProcessPending();
        inst = await ist.Load(inst.Id);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(10, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI|timerNI|timerI|Bookmark2:CONTINUE|timerNI|timerNI|timerNI|timerI", String.Join('|', log));

        inst = await wfe.ResumeAsync(inst.Id, "BookMark2", new ExpandoObject()
        {
            {"Answer", "CANCEL"}
        });
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(12, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI|timerNI|timerI|Bookmark2:CONTINUE|timerNI|timerNI|timerNI|timerI|Bookmark2:CANCEL|end", String.Join('|', log));
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
    }


	[TestMethod]
    public async Task TimerStartVariable()
	{
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\timer\\start_timer_var.bpmn");

        String wfId = "VariableTimer";
        var prms = new ExpandoObject()
        {
            { "StartTime", (DateTime.UtcNow + TimeSpan.FromSeconds(1)) } //.ToString("yyyy-MM-ddTHH\\:mm\\:ss", CultureInfo.InvariantCulture) }
        };

        var wfe = TestEngine.ServiceProvider().GetRequiredService<IWorkflowEngine>();
        var ins = TestEngine.ServiceProvider().GetRequiredService<IInstanceStorage>();

        var inst = await TestEngine.SimpleRun(wfId, xaml, prms);

        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        Thread.Sleep(1100);
        await wfe.ProcessPending();

        var instAfter = await ins.Load(inst.Id);
        var res1 = instAfter.Result;
        Assert.AreEqual("AfterTimer", res1.Get<String>("Result"));
        Assert.AreEqual(WorkflowExecutionStatus.Complete, instAfter.ExecutionStatus);
    }
}

