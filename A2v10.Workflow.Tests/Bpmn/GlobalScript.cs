// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Tests
{
	[TestClass]
	[TestCategory("Bmpn.GlobalScript")]
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
}
