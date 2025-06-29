﻿// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;

namespace A2v10.Workflow.SqlServer.Tests;
[TestClass]
[TestCategory("Bpmn.SqlServer.Events.Message")]
public class BpmnEventsMessage
{
    [TestMethod]
    public async Task BoundaryUninterrapted()
    {
        var boundaryId = "BoundaryMessage";

        await TestEngine.PrepareDatabase(boundaryId);

        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\messages\\boundary.bpmn");
        var inst = await TestEngine.SimpleRun(boundaryId, xaml);
        Assert.IsNotNull(inst);
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(2, log!.Length);
        Assert.AreEqual("startProcess|startSub", String.Join('|', log));

        var sp = TestEngine.ServiceProvider();
        var wfe = sp.GetRequiredService<IWorkflowEngine>();
        inst = await wfe.SendMessageAsync(inst.Id, "Message1");
        Assert.IsNotNull(inst);
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(4, log!.Length);
        Assert.AreEqual("startProcess|startSub|messageBoundary|endBoundary", String.Join('|', log));

        inst = await wfe.ResumeAsync(inst.Id, "MainTask");
        Assert.IsNotNull(inst);
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(6, log!.Length);
        Assert.AreEqual("startProcess|startSub|messageBoundary|endBoundary|endSub|endProcess", String.Join('|', log));
    }

    [TestMethod]
    public async Task BoundaryInterrapted()
    {
        var boundaryId = "BoundaryInterruptedMessage";
        
        await TestEngine.PrepareDatabase(boundaryId);

        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\messages\\boundary_interrupted.bpmn");
        var inst = await TestEngine.SimpleRun(boundaryId, xaml);
        Assert.IsNotNull(inst);
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(2, log!.Length);
        Assert.AreEqual("startProcess|startSub", String.Join('|', log));

        var sp = TestEngine.ServiceProvider();
        var wfe = sp.GetRequiredService<IWorkflowEngine>();
        inst = await wfe.SendMessageAsync(inst.Id, "Message1");
        Assert.IsNotNull(inst);
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(5, log!.Length);
        Assert.AreEqual("startProcess|startSub|messageBoundary|endBoundary|endProcess", String.Join('|', log));
    }

    [TestMethod]
    public async Task IntermediageTimerBookmarks()
    {
        String wfId = "IntermediateMultWithBookmark";
        await TestEngine.PrepareDatabase(wfId);

        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\intermediate_timer_with_bookmarks.bpmn");


        var wfe = TestEngine.ServiceProvider().GetRequiredService<IWorkflowEngine>();

        var inst = await TestEngine.SimpleRun(wfId, xaml);
        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(1, log!.Length);
        Assert.AreEqual("start", String.Join('|', log));

        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        await Task.Delay(1000); // 1
        await wfe.ProcessPending();
        inst = await wfe.LoadInstanceRaw(inst.Id);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(2, log!.Length);
        Assert.AreEqual("start|timerNI", String.Join('|', log));

        await Task.Delay(1000); // 2
        await wfe.ProcessPending();
        inst = await wfe.LoadInstanceRaw(inst.Id);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(3, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI", String.Join('|', log));

        await Task.Delay(1000); // 3
        await wfe.ProcessPending();
        inst = await wfe.LoadInstanceRaw(inst.Id);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(4, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI|timerI", String.Join('|', log));

        inst = await wfe.ResumeAsync(inst.Id, "BookMark2", new ExpandoObject()
            {
                {"Answer", "CONTINUE"}
            });
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(5, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI|timerI|Bookmark2:CONTINUE", String.Join('|', log));
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        await Task.Delay(1000); // 1
        await wfe.ProcessPending();
        inst = await wfe.LoadInstanceRaw(inst.Id);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(6, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI|timerI|Bookmark2:CONTINUE|timerNI", String.Join('|', log));

        await Task.Delay(1000); // 2
        await wfe.ProcessPending();
        inst = await wfe.LoadInstanceRaw(inst.Id);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(7, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI|timerI|Bookmark2:CONTINUE|timerNI|timerNI", String.Join('|', log));

        await Task.Delay(1000); // 3
        await wfe.ProcessPending();
        inst = await wfe.LoadInstanceRaw(inst.Id);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(8, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI|timerI|Bookmark2:CONTINUE|timerNI|timerNI|timerI", String.Join('|', log));

        inst = await wfe.ResumeAsync(inst.Id, "BookMark2", new ExpandoObject()
            {
                {"Answer", "CANCEL"}
            });
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(10, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI|timerI|Bookmark2:CONTINUE|timerNI|timerNI|timerI|Bookmark2:CANCEL|end", String.Join('|', log));
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
    }


    [TestMethod]
    public async Task IntermediateTimerBookmarksDebugDate()
    {
        String wfId = "IntermediateMultWithBookmarkDebugDate";
        await TestEngine.PrepareDatabase(wfId);

        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\events\\intermediate_timer_with_bookmarks.bpmn");


        var wfe = TestEngine.ServiceProvider().GetRequiredService<IWorkflowEngine>();


        var now = DateTime.UtcNow;
        var dt = new DateTime(2022, 03, 16, now.Hour, now.Minute, now.Second, now.Millisecond);

        var dbContext = TestEngine.ServiceProvider().GetRequiredService<IDbContext>();
        await dbContext.ExecuteExpandoAsync(null, "a2wf.[CurrentDate.Set]",
            new ExpandoObject()
            {
                { "Date", dt }
            });

        var inst = await TestEngine.SimpleRun(wfId, xaml);
        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(1, log!.Length);
        Assert.AreEqual("start", String.Join('|', log));

        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        await Task.Delay(1050); // 1
        await wfe.ProcessPending();
        inst = await wfe.LoadInstanceRaw(inst.Id);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(2, log!.Length);
        Assert.AreEqual("start|timerNI", String.Join('|', log));

        await Task.Delay(1020); // 2
        await wfe.ProcessPending();
        inst = await wfe.LoadInstanceRaw(inst.Id);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(3, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI", String.Join('|', log));

        await Task.Delay(1020); // 3
        await wfe.ProcessPending();
        inst = await wfe.LoadInstanceRaw(inst.Id);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(4, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI|timerI", String.Join('|', log));

        inst = await wfe.ResumeAsync(inst.Id, "BookMark2", new ExpandoObject()
            {
                {"Answer", "CONTINUE"}
            });
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(5, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI|timerI|Bookmark2:CONTINUE", String.Join('|', log));
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        await Task.Delay(1020); // 1
        await wfe.ProcessPending();
        inst = await wfe.LoadInstanceRaw(inst.Id);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(6, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI|timerI|Bookmark2:CONTINUE|timerNI", String.Join('|', log));

        await Task.Delay(1020); // 2
        await wfe.ProcessPending();
        inst = await wfe.LoadInstanceRaw(inst.Id);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(7, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI|timerI|Bookmark2:CONTINUE|timerNI|timerNI", String.Join('|', log));

        await Task.Delay(1020); // 3
        await wfe.ProcessPending();
        inst = await wfe.LoadInstanceRaw(inst.Id);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(8, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI|timerI|Bookmark2:CONTINUE|timerNI|timerNI|timerI", String.Join('|', log));

        inst = await wfe.ResumeAsync(inst.Id, "BookMark2", new ExpandoObject()
            {
                {"Answer", "CANCEL"}
            });
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(10, log!.Length);
        Assert.AreEqual("start|timerNI|timerNI|timerI|Bookmark2:CONTINUE|timerNI|timerNI|timerI|Bookmark2:CANCEL|end", String.Join('|', log));
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
    }
}

