// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Jint;
using Jint.Native;
using Jint.Runtime.Interop;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Tests
{
	[TestClass]
	[TestCategory("ScriptEngine")]
	public class TestScript
	{

		const String program = @"
() => {
	var _fmap_ = {};

	_fmap_['Ref0'] = {
		Script: () => 7
	};

	return _fmap_;
};
";

		[TestMethod]
		public void ScriptEngine()
		{
			var eng = new Engine(opts =>
			{
				opts.Strict(true);
			});


			var obj = eng.Evaluate(program).ToObject();
			var func = obj as Func<JsValue, JsValue[], JsValue>;

			var val = func.Invoke(JsValue.Undefined, null).ToObject();

			if (val is IDictionary<String, Object> dict)
			{
				var x = dict["Ref0"] as IDictionary<String, Object>;
				var res = JsValue.FromObject(eng, x["Script"]).Invoke();
				Assert.AreEqual("7", res.ToString());
			}
			else
			{
				Assert.Fail("Ref0 not found");
			}

		}

		[TestMethod]
		public void ScriptDatabase()
		{
			var eng = new Engine(opts =>
			{
				opts.Strict(true);
				opts.SetWrapObjectHandler((e, o) =>
				{
					if (o is IInjectable injectable)
						injectable.Inject(null);
					return new ObjectWrapper(e, o);
				});
			});
			eng.AddNativeObjects(new ScriptNativeObjects());

			var val = eng.Evaluate("return (new Database()).loadModel('first')").ToObject();
			var eo = val as ExpandoObject;
			Assert.AreEqual("value", eo.Get<String>("prop"));
			Assert.AreEqual("first", eo.Get<String>("procedure"));
		}

		[TestMethod]
		public void ScriptDatabase2()
		{
			var eng = new Engine(opts =>
			{
				opts.Strict(true);
				opts.SetWrapObjectHandler((e, o) =>
				{
					if (o is IInjectable injectable)
						injectable.Inject(null);
					return new ObjectWrapper(e, o);
				});
			});
			eng.AddNativeObjects(new ScriptNativeObjects());

			var val = eng.Evaluate("var x = (new Database()).loadModel('first'); let ix=''; for(let p in x) ix+=p; return ix;").ToObject();
			// keys?
		}
	}
}
