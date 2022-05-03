// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace A2v10.Workflow.Tests.Simple
{
    [TestClass]
    [TestCategory("Simple.Flow")]
    public class SimpleFlow
    {
        [TestMethod]
        public async Task Sequence()
        {
            var root = new Sequence()
            {
                Id = "Ref0",
                Variables = new List<IVariable>{
                    new Variable() {Name = "X", Dir = VariableDirection.In, Type=VariableType.Number},
                    new Variable() {Name = "R", Dir = VariableDirection.Out, Type=VariableType.Number}
                },
                Activities = new List<IActivity> {
                    new Code() {Id="Ref1", Script="X = X + 1"},
                    new Code() {Id="Ref2", Script="X = X + 1"},
                    new Code() {Id="Ref3", Script="R = X"},
                }
            };

            var wfe = TestEngine.CreateInMemoryEngine();
            var inst = await wfe.CreateAsync(root, null);
            inst = await wfe.RunAsync(inst.Id, new { X = 5 });
            var result = inst.Result;
            Assert.AreEqual(7, result.Get<Int32>("R"));

            inst = await wfe.CreateAsync(root, null);
            inst = await wfe.RunAsync(inst.Id, new { X = 3 });
            result = inst.Result;
            Assert.AreEqual(5, result.Get<Int32>("R"));

            inst = await wfe.CreateAsync(root, null);
            inst = await wfe.RunAsync(inst.Id, new { X = 11 });
            Assert.AreEqual(13, inst.Result.Get<Int32>("R"));
        }

        [TestMethod]
        public async Task Parallel()
        {
            var root = new Parallel()
            {
                Id = "Ref0",
                Branches = new List<IActivity> {
                    new Code() {Id="Ref1", Script="console.log('ref1')"},
                    new Code() {Id="Ref2", Script="console.log('ref2')"},
                    new Code() {Id="Ref3", Script="console.log('ref3')"},
                }
            };

            var wfe = TestEngine.CreateInMemoryEngine();
            var inst = await wfe.CreateAsync(root, null);
            await wfe.RunAsync(inst.Id);
        }

        [TestMethod]
        public async Task If()
        {
            var root = new Sequence()
            {
                Id = "Ref0",
                Variables = new List<IVariable> {
                    new Variable() {Name = "X", Dir = VariableDirection.In, Type=VariableType.Number},
                    new Variable() {Name = "R", Dir = VariableDirection.Out, Type=VariableType.String}
                },
                Activities = new List<IActivity>{
                    new If() {
                        Id="Ref1",
                        Condition="X > 5",
                        Then = new Code() {Id="Ref2", Script = "R = 'X > 5'"},
                        Else = new Code() {Id="Ref3", Script = "R = 'X <= 5'"}
                    }
                }
            };

            var wfe = TestEngine.CreateInMemoryEngine();
            var inst = await wfe.CreateAsync(root, null);
            inst = await wfe.RunAsync(inst.Id, new { X = 4 });
            Assert.AreEqual("X <= 5", inst.Result.Get<String>("R"));

            inst = await wfe.CreateAsync(root, null);
            inst = await wfe.RunAsync(inst.Id, new { X = 23 });
            Assert.AreEqual("X > 5", inst.Result.Get<String>("R"));
        }


        [TestMethod]
        public async Task NestedContext()
        {
            var root = new Sequence()
            {
                Id = "Ref0",
                Variables = new List<IVariable>{
                    new Variable() {Name = "X", Dir = VariableDirection.In, Type=VariableType.Number},
                    new Variable() {Name = "R", Dir = VariableDirection.Out, Type=VariableType.Number},
                },
                Activities = new List<IActivity> {
                    new Code() {Id="Ref1", Script="X = X + 1"},
                    new Code() {Id="Ref2", Script="X = X + 1"},
                    new Sequence() { Id="Ref3",
                        Activities = new List<IActivity> {
                            new Code() {Id="Ref4", Script="X = X + 1"},
                            new Code() {Id="Ref5", Script="X = X + 1"},
                        }
                    },
                    new Code() {Id="Ref7", Script="R = X"},
                }
            };

            var wfe = TestEngine.CreateInMemoryEngine();
            var inst = await wfe.CreateAsync(root, null);
            inst = await wfe.RunAsync(inst.Id, new { X = 5 });
            var result = inst.Result;
            Assert.AreEqual(9, result.Get<Int32>("R"));
        }
    }
}
