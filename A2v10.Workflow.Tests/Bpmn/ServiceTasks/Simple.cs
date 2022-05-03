// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace A2v10.Workflow.Tests
{
    [TestClass]
    [TestCategory("Bpmn.ServiceTasks")]
    public class BpmnServiceTasks
    {
        [TestMethod]
        public async Task CallApi()
        {
            var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\servicetask\\callapi.bpmn");
            String wfId = "CallApi";
            var inst = await TestEngine.SimpleRun(wfId, xaml, null);

            //var res0 = inst.Result;
            Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        }
    }
}
