// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace A2v10.Workflow.Tests
{
    [TestClass]
    [TestCategory("Simple.StateMachine")]
    public class SimpleStateMachine
    {
        [TestMethod]
        public async Task Simple()
        {
            var root = new StateMachine()
            {
                Id = "Ref0",
                Variables = new List<IVariable>
                {
                    new Variable() {Name = "X", Dir = VariableDirection.InOut, Type=VariableType.Number},
                },
                States = new List<StateBase>()
                {
                    new StartState() {Id="Ref1", Next="Ref2"},
                    new State() {Id="Ref2", Next="Ref3",
                        Entry = new Code() {Id="Ref21", Script = "X +=1"},
                    },
                    new FinalState {Id="Ref3"}
                }
            };

            var wfe = TestEngine.CreateInMemoryEngine();
            var inst = await wfe.CreateAsync(root, null);
            inst = await wfe.RunAsync(inst.Id, new { X = 5 });
            var result = inst.Result;
            Assert.AreEqual(6, result.Get<Int32>("X"));
        }

        [TestMethod]
        public async Task Counter()
        {
            var root = new StateMachine()
            {
                Id = "Ref0",
                Variables = new List<IVariable>
                {
                    new Variable() {Name = "X", Dir = VariableDirection.InOut, Type=VariableType.Number},
                },
                States = new List<StateBase>()
                {
                    new StartState() {Id="Ref1", Next="Ref2"},
                    new State() {Id="Ref2", Next="Ref3",
                        Entry = new Code() {Id="Ref21", Script = "X -=1"},
                        Transitions = new List<Transition>() {
                            new Transition() { Id = "Ref211", Condition="X > 0", Destination="Ref2" }
                        }
                    },
                    new FinalState {Id="Ref3"}
                }
            };

            var wfe = TestEngine.CreateInMemoryEngine();
            var inst = await wfe.CreateAsync(root, null);
            inst = await wfe.RunAsync(inst.Id, new { X = 5 });
            var result = inst.Result;
            Assert.AreEqual(0, result.Get<Int32>("X"));
        }
    }
}
