// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace A2v10.Workflow.Tests
{
    [TestClass]
    [TestCategory("Bpmn.Subprocess")]
    public class BpmnSubprocess
    {
        [TestMethod]
        public async Task Simple()
        {
            var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\subprocess\\subprocess_simple.bpmn");

            String wfId = "SimpleSub";
            var inst = await TestEngine.SimpleRun(wfId, xaml);

            var res0 = inst.Result;
            Assert.AreEqual(10, res0.Get<Double>("X"));
            Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
            Assert.IsNotNull(res0);
            var log = res0!.GetNotNull<Object[]>("Log");
            Assert.AreEqual(5, log.Length);
            Assert.AreEqual("start|startSub|task|endSub|end", String.Join('|', log));
        }

        [TestMethod]
        public async Task Loop()
        {
            var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\subprocess\\subprocess_loop.bpmn");

            String wfId = "SimpleSubLoop";
            var inst = await TestEngine.SimpleRun(wfId, xaml);

            var res0 = inst.Result;
            Assert.AreEqual(20, res0.Get<Double>("X"));
            Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
            var log = res0.Get<Object[]>("Log");
            Assert.IsNotNull(log);
            Assert.AreEqual(11, log!.Length);
            Assert.AreEqual("start|startSub|task|endSub|startSub|task|endSub|startSub|task|endSub|end", String.Join('|', log));
        }

        [TestMethod]
        public async Task LoopMultTokens()
        {
            var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\subprocess\\subprocess_loop_multokens.bpmn");

            String wfId = "SimpleSubLoop";
            var inst = await TestEngine.SimpleRun(wfId, xaml);

            var res0 = inst.Result;
            Assert.AreEqual(29, res0.Get<Double>("X"));
            Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
            var log = res0.Get<Object[]>("Log");
            Assert.IsNotNull(log);
            Assert.AreEqual(17, log!.Length);
            Assert.AreEqual("start|startSub|task1|task2|endSub1|endSub2|startSub|task1|task2|endSub1|endSub2|startSub|task1|task2|endSub1|endSub2|end", String.Join('|', log));
        }

        [TestMethod]
        public async Task LoopMultiTokensBookmark()
        {
            var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\subprocess\\subprocess_loop_multokens_bookmark.bpmn");

            String wfId = "SimpleSubLoop";
            var inst = await TestEngine.SimpleRun(wfId, xaml);

            {
                var res0 = inst.Result;
                Assert.AreEqual(13, res0.Get<Double>("X"));
                Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
                var log = res0.Get<Object[]>("Log");
                Assert.IsNotNull(log);
                Assert.AreEqual(5, log!.Length);
                Assert.AreEqual("start|startSub|task1|task2|endSub2", String.Join('|', log));
            }

            var eng = TestEngine.ServiceProvider().GetRequiredService<IWorkflowEngine>();
            {
                var inst1 = await eng.ResumeAsync(inst.Id, "Bookmark1", null);
                var res1 = inst1.Result;
                Assert.AreEqual(21, res1.Get<Double>("X"));
                Assert.AreEqual(WorkflowExecutionStatus.Idle, inst1.ExecutionStatus);
                var log1 = res1.Get<Object[]>("Log");
                Assert.IsNotNull(log1);
                Assert.AreEqual(10, log1!.Length);
                Assert.AreEqual("start|startSub|task1|task2|endSub2|endSub1|startSub|task1|task2|endSub2", String.Join('|', log1));
            }

            {
                var inst2 = await eng.ResumeAsync(inst.Id, "Bookmark1", null);
                Assert.AreEqual(WorkflowExecutionStatus.Idle, inst2.ExecutionStatus);
                var res2 = inst2.Result;
                Assert.IsNotNull(res2);
                var log1 = res2!.GetNotNull<Object[]>("Log");
                Assert.AreEqual(15, log1!.Length);
                Assert.AreEqual("start|startSub|task1|task2|endSub2|endSub1|startSub|task1|task2|endSub2|endSub1|startSub|task1|task2|endSub2", String.Join('|', log1));
                Assert.AreEqual(29, res2.Get<Double>("X"));
            }

            {
                var inst3 = await eng.ResumeAsync(inst.Id, "Bookmark1", null);
                var res3 = inst3.Result;
                Assert.AreEqual(39, res3.Get<Double>("X"));
                Assert.AreEqual(WorkflowExecutionStatus.Complete, inst3.ExecutionStatus);
                Assert.IsNotNull(res3);
                var log1 = res3!.GetNotNull<Object[]>("Log");
                Assert.AreEqual(18, log1!.Length);
                Assert.AreEqual("start|startSub|task1|task2|endSub2|endSub1|startSub|task1|task2|endSub2|endSub1|startSub|task1|task2|endSub2|endSub1|task3|end", String.Join('|', log1));
            }
        }

        [TestMethod]
        public async Task Nested()
        {
            var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\subprocess\\subprocess_nested.bpmn");

            String wfId = "SimpleSub";
            var inst = await TestEngine.SimpleRun(wfId, xaml);

            var res0 = inst.Result;
            Assert.AreEqual(25, res0.Get<Double>("X"));
            Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
            Assert.IsNotNull(res0);
            var log = res0!.GetNotNull<Object[]>("Log");
            Assert.AreEqual(8, log!.Length);
            Assert.AreEqual("start|startSub|task|startNestedSub|nestedTask|endNestedSub|endSub|end", String.Join('|', log));
        }

    }
}
