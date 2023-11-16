// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace A2v10.Workflow.Tests.Runtime
{
    [TestClass]
    [TestCategory("Runtime.Store")]
    public class StoreActivity
    {
        [TestMethod]
        public async Task StoreSequence()
        {
            IActivity root = new Sequence()
            {
                Id = "Ref0",
                Variables =
                [
                    new Variable() {Name = "x", Dir= VariableDirection.InOut, Type=VariableType.Number}
                ],
                Activities =
                [
                    new Code() {Id="Ref1", Script="x += 5"},
                    new Wait() {Id="Ref2", Bookmark="Bookmark1"},
                    new Code() {Id="Ref3", Script="x += 5"},
                ]
            };

            var wfe = TestEngine.CreateInMemoryEngine();
            var inst = await wfe.CreateAsync(root, null);
            inst = await wfe.RunAsync(inst.Id, new { x = 5 });
            Assert.AreEqual(10, inst.Result.Get<Int32>("x"));

            var resInst = await wfe.ResumeAsync(inst.Id, "Bookmark1");
            Assert.AreEqual(15, resInst.Result.Get<Int32>("x"));
        }
    }
}
