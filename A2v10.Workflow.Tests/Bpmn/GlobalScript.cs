// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace A2v10.Workflow.Tests;

[TestClass]
[TestCategory("Bpmn.GlobalScript")]
public class TestGlobalScript
{
    [TestMethod]
    public async Task GlobalScriptProcess()
    {
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\globalscript_process.bpmn");

        String wfId = "GlobalScriptProcess";
        var res = await TestEngine.SimpleRun(wfId, xaml);
        Assert.AreEqual("RESULT", res.Result.Get<String>("S"));
    }
}

