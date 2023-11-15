// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.System.Xaml;
using A2v10.Workflow.Bpmn;
using A2v10.Workflow.Interfaces;
using A2v10.Workflow.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace A2v10.Workflow.Tests.Serialization
{
    [TestClass]
    [TestCategory("Serialization.Instance")]
    public class SerializeInstance
    {
        [TestMethod]
        public void SimpleProcessJson()
        {
            var p = new Process()
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
                    new EndEvent()
                    {
                        Id = "end",
                        Children = [new Incoming() {Text = "script->end"}]
                    },
                    new SequenceFlow()
                    {
                        Id = "start->end",
                        SourceRef = "start",
                        TargetRef = "end"
                    }
                ]
            };

            var s = new WorkflowSerializer(new XamlReaderService());

            var json = s.SerializeActitity(p, "json");

            var r = s.DeserializeActitity(json, "json").Activity as Process;
            Assert.IsNotNull(r);

            Assert.AreEqual(r!.Id, p.Id);
            Assert.AreEqual(r.Children!.Count, p.Children.Count);

            var pEvent = p.FindElement<Event>("start");
            var rEvent = r.FindElement<Event>("start");
            Assert.AreEqual(pEvent.Id, rEvent.Id);
            Assert.AreEqual(pEvent.IsStart, rEvent.IsStart);

            Console.WriteLine(json);
        }

        [TestMethod]
        public void SimpleSequenceJson()
        {
            Sequence s = new()
            {
                Id = "Ref0",
                Variables =
                [
                    new Variable() {Name = "x", Dir= VariableDirection.InOut}
                ],
                Activities =
                [
                    new Code() {Id="Ref1", Script="x += 5"},
                    new Wait() {Id="Ref2", Bookmark="Bookmark1"},
                    new Code() {Id="Ref3", Script="x += 5"},
                ]
            };

            var ser = new WorkflowSerializer(null);
            var json = ser.SerializeActitity(s, "json");

            var r = ser.DeserializeActitity(json, "json").Activity as Sequence;

            Assert.IsNotNull(r);
            Assert.AreEqual(r!.Id, s.Id);
            Assert.AreEqual(r.Variables!.Count, s.Variables.Count);
            Assert.AreEqual(r.Activities!.Count, s.Activities.Count);

            Assert.AreEqual(r.Activities[0].Id, s.Activities[0].Id);
            Assert.AreEqual(r.Activities[1].Id, s.Activities[1].Id);
            Assert.AreEqual(r.Activities[0].GetType(), typeof(Code));
            Assert.AreEqual(r.Activities[1].GetType(), typeof(Wait));
            Assert.AreEqual(r.Activities[2].GetType(), typeof(Code));
        }
    }
}
