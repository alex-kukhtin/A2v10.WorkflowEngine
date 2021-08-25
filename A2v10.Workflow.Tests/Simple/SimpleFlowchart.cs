// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Tests
{
	[TestClass]
	[TestCategory("Simple.Flowchart")]
	public class SimpleFlowchart
	{
		[TestMethod]
		public async Task Simple()
		{
			var root = new Flowchart()
			{
				Id = "Ref0",
				Variables = new List<IVariable>
				{
					new Variable() {Name = "X", Dir = VariableDirection.InOut, Type=VariableType.Number},
				},
				Nodes = new List<FlowNode>()
				{
					new FlowStart() {Id="Ref1", Next="Ref2"},
					new FlowDecision() {Id="Ref2", Condition="X > 0", Then = "Ref3"},
					new FlowActivity() {Id = "Ref3", Next="Ref2",
						Activity = new Code() {Id="Ref4", Script="X -= 1" }
					}
				}
			};

			var wfe = TestEngine.CreateInMemoryEngine();
			var inst = await wfe.CreateAsync(root, null);
			inst = await wfe.RunAsync(inst.Id, new { X = 5 });
			var result = inst.Result;
			Assert.AreEqual(0, result.Get<Int32>("X"));

			inst = await wfe.CreateAsync(root, null);
			inst = await wfe.RunAsync(inst.Id, new { X = 3 });
			result = inst.Result;
			Assert.AreEqual(0, result.Get<Int32>("X"));
		}

		[TestMethod]
		public async Task Bookmarks()
		{
			var root = new Flowchart()
			{
				Id = "Ref0",
				Variables = new List<IVariable>
				{
					new Variable() {Name = "X", Dir = VariableDirection.InOut, Type=VariableType.Number},
				},
				Nodes = new List<FlowNode>()
				{
					new FlowStart() {Id="Ref1", Next="Ref2"},
					new FlowDecision() {Id="Ref2", Condition="X > 0", Then = "Ref3"},
					new FlowActivity() {Id = "Ref3", Next="Ref2",
						Activity = new Sequence() {
							Id="Ref4",
							Activities = new List<IActivity>() {
								new Wait() {Id="Ref5", Bookmark="BM1" },
								new Code() {Id="Ref6", Script="X -= 1" }
							}
						}
					}
				}
			};

			var wfe = TestEngine.CreateInMemoryEngine();
			var inst = await wfe.CreateAsync(root, null);
			inst = await wfe.RunAsync(inst.Id, new { X = 5 });
			var result = inst.Result;
			Assert.AreEqual(5, result.Get<Int32>("X"));
			//var id = inst.Id;

			int x = result.Get<Int32>("X");
			while (x > 0)
			{
				var resInst = await wfe.ResumeAsync(inst.Id, "BM1");
				x = resInst.Result.Get<Int32>("X");
			}
			Assert.AreEqual(0, x);
		}
	}
}
