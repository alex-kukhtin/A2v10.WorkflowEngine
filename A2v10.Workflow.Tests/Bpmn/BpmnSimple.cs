// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

using A2v10.Workflow.Bpmn;
using A2v10.Workflow.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace A2v10.Workflow.Tests
{
    [TestClass]
    [TestCategory("Bpmn.Simple")]
    public class BpmnSimple
    {
        [TestMethod]
        public async Task Sequence()
        {
            var process = new Process()
            {
                Id = "process",
                IsExecutable = true,
                Children =
                [
                    new StartEvent()
                    {
                        Id = "start",
                        Children = [new Outgoing() {Text = "start->script" }]
                    },
                    new ScriptTask()
                    {
                        Id = "script",
                        Children  = [
                            new Incoming() { Text = "start->script" },
                            new Outgoing() { Text = "script->end" }
                        ]
                    },
                    new EndEvent()
                    {
                        Id = "end",
                        Children = [new Incoming() { Text = "script->end" }]
                    },
                    new SequenceFlow()
                    {
                        Id = "start->script",
                        SourceRef = "start",
                        TargetRef = "script"
                    },
                    new SequenceFlow()
                    {
                        Id = "script->end",
                        SourceRef = "script",
                        TargetRef = "end"
                    }
                ]
            };

            var wfe = TestEngine.CreateInMemoryEngine();
            var inst = await wfe.CreateAsync(process, null);
            inst = await wfe.RunAsync(inst.Id);
            Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        }


        [TestMethod]
        public async Task ParallelGateway()
        {
            var process = new Process()
            {
                Id = "process",
                IsExecutable = true,
                Children =
                [
                    new StartEvent()
                    {
                        Id = "start",
                        Children = [new Outgoing() {Text = "start->gate1" }]
                    },
                    new ParallelGateway()
                    {
                        Id = "gate1",
                        Children  = [
                            new Incoming() {Text = "start->gate1" },
                            new Outgoing() { Text = "gate1->task1" },
                            new Outgoing() { Text = "gate1->task2" },
                            new Script() {Text = ""}
                        ]
                    },
                    new ScriptTask()
                    {
                        Id = "task1",
                        Children  = [
                            new Incoming() {Text = "gate1->task1" },
                            new Outgoing() { Text = "task1->gate2" }
                        ]
                    },
                    new ScriptTask()
                    {
                        Id = "task2",
                        Children  = [
                            new Incoming() {Text ="gate1->task2" },
                            new Outgoing() {Text = "task2->gate2" }
                        ]
                    },
                    new ParallelGateway()
                    {
                        Id = "gate2",
                        Children  = [
                            new Incoming() { Text = "task1->gate2" },
                            new Incoming() {Text = "task2->gate2" },
                            new Outgoing() {Text = "gate2->end", }
                        ],
                    },
                    new EndEvent()
                    {
                        Id = "end",
                        Children = [new Incoming() {Text ="script->end"}]
                    },
                    new SequenceFlow() {Id = "start->gate1", SourceRef = "start", TargetRef = "gate1"},
                    new SequenceFlow() {Id = "gate1->task1", SourceRef = "gate1", TargetRef = "task1"},
                    new SequenceFlow() {Id = "gate1->task2", SourceRef = "gate1", TargetRef = "task2"},
                    new SequenceFlow() {Id = "task1->gate2", SourceRef = "task1", TargetRef = "gate2"},
                    new SequenceFlow() {Id = "task2->gate2", SourceRef = "task2", TargetRef = "gate2"},
                    new SequenceFlow() {Id = "gate2->end",   SourceRef = "gate2", TargetRef = "end"}
                ]
            };

            var wfe = TestEngine.CreateInMemoryEngine();
            var inst = await wfe.CreateAsync(process, null);
            inst = await wfe.RunAsync(inst.Id);
            Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        }
    }
}
