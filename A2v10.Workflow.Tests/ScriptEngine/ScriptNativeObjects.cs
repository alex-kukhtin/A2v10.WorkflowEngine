// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace A2v10.Workflow.Tests
{
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
    }

    public class ScriptNativeObjects : IScriptNativeObjectProvider
    {
        public IEnumerable<NativeType> NativeTypes()
        {
            yield return new NativeType(Name: "Database", Type: typeof(ScriptDatabase));
            yield return new NativeType(Name: "Deferred", Type: typeof(ScriptNativeDeferred));
        }
    }
}
