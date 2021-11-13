// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Jint;
using Jint.Native;
using Jint.Runtime.Interop;

using A2v10.Workflow.Interfaces;
using Newtonsoft.Json;
using A2v10.Workflow.Serialization;

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
				var res = eng.Invoke(JsValue.FromObject(eng, x["Script"]));
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


		const String programCount = @"
() => {
	""use strict"";
	var _fmap_ = {};

	let R; let Count = 0;

	(function() {
		_fmap_['Ref0'] = {
			Script: () => Count += 1
		};

		_fmap_['Ref1'] = {
			Script: () => {R = Count.toString();},
			Result: () => {return {R: R};}
		}
	})();

	return _fmap_;
};
";

		[TestMethod]
		public void ScriptEngineCountError()
		{
			var eng = new Engine(opts =>
			{
				opts.Strict(true);
			});


			var obj = eng.Evaluate(programCount).ToObject();
			var func = obj as Func<JsValue, JsValue[], JsValue>;

			var val = func.Invoke(JsValue.Undefined, null).ToObject();

			if (val is IDictionary<String, Object> dict)
			{
				var ref0 = dict["Ref0"] as IDictionary<String, Object>;
				var res = eng.Invoke(JsValue.FromObject(eng, ref0["Script"]));
				Assert.AreEqual((Double)1, res.AsNumber());

				var ref1 = dict["Ref1"] as IDictionary<String, Object>;
				var res2 = eng.Invoke(JsValue.FromObject(eng, ref1["Script"]));
				var res3 = eng.Invoke(JsValue.FromObject(eng, ref1["Result"]));
				Assert.AreEqual("1", res3.AsObject().Get("R"));
			}
		}

		const String ScriptProgramError = @"
    ""use strict"";
() => {
let __fmap__ = { };
		(function() {
			let R; let SumSaldo; let Count = 0;
		__fmap__['Process_1'] = {
Result: () => {return { R : R
	};
},
Store: () => { return { R: R, SumSaldo: SumSaldo, Count: Count }; },
Restore: (_arg_) => { R = _arg_.R; SumSaldo = _arg_.SumSaldo; Count = _arg_.Count; }
};
__fmap__['CheckSaldo'] = {
Script: (reply) => { SumSaldo = reply.Answer; ; }
};
__fmap__['Activity_0p7ly0p'] = {
Script: () => { R = Count.toString(); ; }
};
__fmap__['Flow_0v13tb0'] = {
ConditionExpression: () => { return SumSaldo > 0; }
};
__fmap__['CountPlus'] = {
Script: () => { Count += 1; ; }
};
})();
return __fmap__;
};
";

		[TestMethod]
		public void ScriptEngineProgramError()
		{
			var eng = new Engine(opts =>
			{
				opts.Strict(true);
			});


			var obj = eng.Evaluate(ScriptProgramError).ToObject();
			var func = obj as Func<JsValue, JsValue[], JsValue>;

			var val = func.Invoke(JsValue.Undefined, null).ToObject();

			if (val is not IDictionary<String, Object> dict)
				throw new ArgumentException("Invalid type");

			var refP = dict["Process_1"] as IDictionary<String, Object>;
			var resStore = eng.Invoke(JsValue.FromObject(eng, refP["Store"]));
			var resStoreVal = resStore.ToObject();
			dict = null;

			var eng2 = new Engine(opts =>
			{
				opts.Strict(true);
			});

			obj = eng.Evaluate(ScriptProgramError).ToObject();
			func = obj as Func<JsValue, JsValue[], JsValue>;

			val = func.Invoke(JsValue.Undefined, null).ToObject();

			if (val is not IDictionary<String, Object> dict2)
				throw new ArgumentException("Invalid type");

			refP = dict2["Process_1"] as IDictionary<String, Object>;
			var restoreFunc = (Func<JsValue, JsValue[], JsValue>)refP["Restore"];
			restoreFunc(null, new JsValue[] { resStore });

			var refCP = dict2["CountPlus"] as IDictionary<String, Object>;
			var res = eng.Invoke(JsValue.FromObject(eng, refCP["Script"])).ToObject();

			resStoreVal = eng.Invoke(JsValue.FromObject(eng, refP["Store"])).ToObject();

			var refCalc = dict2["Flow_0v13tb0"] as IDictionary<String, Object>;
			var res2x = eng.Invoke(JsValue.FromObject(eng, refCalc["ConditionExpression"]));


			var refA = dict2["Activity_0p7ly0p"] as IDictionary<String, Object>;
			eng.Invoke(JsValue.FromObject(eng, refA["Script"])).ToObject();

			var resFinal = eng.Invoke(JsValue.FromObject(eng, refP["Store"]));
			Assert.AreEqual("1", resFinal.AsObject().Get("R"));

			/*
			var ref1 = dict["Ref1"] as IDictionary<String, Object>;
			var res2 = JsValue.FromObject(eng, ref1["Script"]).Invoke();
			var res3 = JsValue.FromObject(eng, ref1["Result"]).Invoke();
			*/
		}


		[TestMethod]
		public void DeserializeStringArrays()
		{
			var eng = new Engine(opts =>
			{
				opts.Strict(true);
			});

			var strArray = "{x: [\"f\", \"2\"]}";
			var arg = JsonConvert.DeserializeObject<ExpandoObject>(strArray, new ExpandoObjectConverterArray());

			var obj = eng.Evaluate("return function test(arg) { let r = arg.x; r.push('z'); return JSON.stringify(r); }").ToObject();
			var func = obj as Func<JsValue, JsValue[], JsValue>;
			var arr = func.Invoke(null, new JsValue[] { JsValue.FromObject(eng, arg) });
			Assert.AreEqual("[\"f\",\"2\",\"z\"]", arr.ToString());
		}
	}
}
