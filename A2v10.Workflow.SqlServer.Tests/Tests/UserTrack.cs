// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;

namespace A2v10.Workflow.SqlServer.Tests;
[TestClass]
[TestCategory("Bpmn.SqlServer.Events.Message")]
public class BpmnUserTrack
{
    [TestMethod]
    public async Task TrackScript()
    {
        var boundaryId = "TrackScript";

        await TestEngine.PrepareDatabase(boundaryId);

        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\Track\\Track_Script.bpmn");
        var inst = await TestEngine.SimpleRun(boundaryId, xaml);
        Assert.IsNotNull(inst);
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);

        var dm = await TestEngine.GetTrackAsync(inst.Id);
        var list = dm.Eval<List<ExpandoObject>>("Track")
            ?? throw new InvalidOperationException("List is null");

        Assert.HasCount(3, list);
        Assert.AreEqual("start", list[0].Get<String>("Message"));
        Assert.AreEqual("script", list[1].Get<String>("Message"));
        Assert.AreEqual("end", list[2].Get<String>("Message"));

        Assert.IsNotNull(dm);

    }

}

