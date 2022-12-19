// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace A2v10.Workflow.Tests
{

    public class ScriptNativeArguments : IInjectable
    {
        private readonly String _arg1;
        private readonly Int32 _arg2;
        private readonly Int64 _arg3;

        public ScriptNativeArguments(String arg1, Int32 arg2, Int64 arg3)
        {
            _arg1 = arg1;
            _arg2 = arg2;
            _arg3 = arg3;
        }

        public void Inject(IServiceProvider serviceProvider)
        {
        }

        public void SetDeferred(IDeferredTarget deferredTarget)
        {
        }

        public String Argument1() { return _arg1; }
        public Int32 Argument2() { return _arg2; }
        public Int64 Argument3 => _arg3;
    }

    public class ScriptDatabase : IInjectable
    {
        private readonly ExpandoObject _eo = new();
        public ScriptDatabase()
        {
        }
#pragma warning disable IDE1006 // Naming Styles
        public ExpandoObject loadModel(String procedure, ExpandoObject? prms = null)
#pragma warning restore IDE1006 // Naming Styles
        {
            _eo.Set("prop", "value");
            _eo.Set("procedure", procedure);
            _eo.Set("params", prms);
            return _eo;
        }

        public void Inject(IServiceProvider serviceProvider)
        {
            _eo.Set("injected", true);
        }

		public void SetDeferred(IDeferredTarget deferredTarget)
        {
        }
	}

	public class ScriptNativeObjects : IScriptNativeObjectProvider
    {
        public IEnumerable<NativeType> NativeTypes()
        {
            yield return new NativeType(Name: "Database", Type: typeof(ScriptDatabase));
            yield return new NativeType(Name: "Deferred", Type: typeof(ScriptNativeDeferred));
            yield return new NativeType(Name: "Args", Type: typeof(ScriptNativeArguments));
        }
    }
}
