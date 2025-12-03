// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

using System.Collections.Generic;
using System.Dynamic;

using Microsoft.Extensions.DependencyInjection;

using Jint;
using Jint.Native;
using Jint.Runtime.Interop;

using A2v10.Workflow.Tracker;

namespace A2v10.Workflow;

public class ScriptEngine
{
    private readonly Engine _engine;
    private readonly ExpandoObject? _scriptData;
    private readonly IActivity _root;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITracker _tracker;
    private readonly WorkflowDeferred _deferredTarget = new();

    private IDictionary<String, Object?> ScriptData => _scriptData ?? throw new InvalidProgramException("ScriptData is null");

    public ScriptEngine(IServiceProvider serviceProvider, ITracker tracker, IActivity root, String script, IInstance instance, Object? args = null)
    {
        _root = root;
        _serviceProvider = serviceProvider;
        _tracker = tracker;
        _engine = new Engine(EngineOptions);

        var nativeObjects = _serviceProvider.GetService<IScriptNativeObjectProvider>();
        if (nativeObjects != null)
            _engine.AddNativeObjects(nativeObjects);
        _engine.SetValue("Instance", new ExpandoObject()
        {
            { "Id", instance.Id },
            { "CorrelationId", instance.CorrelationId },
            { "ExecutionStatus", instance.ExecutionStatus.ToString() }
        });
        _engine.SetValue("_loadPersistent", LoadPersistentValue);
        _engine.SetValue("_savePersistent", SavePersistentValue);
        _engine.SetValue("LastResult", instance.State.Get<Object>("LastResult"));
        _engine.SetValue("Host", new JsWorkflowHost(_serviceProvider));
        //Console.WriteLine(script);

        var func = _engine.Evaluate(script);
        if (!func.IsUndefined())
            _scriptData = _engine.Invoke(func).ToObject() as ExpandoObject;
        if (args != null)
            SetArguments(args);
    }

    private void EngineOptions(Options opts)
    {
        opts.Strict(true);
        opts.SetWrapObjectHandler((e, o, tp) =>
        {
            if (o is IInjectable injectable)
            {
                injectable.Inject(_serviceProvider);
                injectable.SetDeferred(_deferredTarget);
            }
            return ObjectWrapper.Create(e, o);
        });
    }

    void SetArguments(Object args)
    {
        if (args is ExpandoObject eo && eo.IsEmpty())
            return;
        var func = GetFunc(_root.Id, "Arguments");
        func?.Invoke(JsValue.Undefined, [JsValue.FromObject(_engine, args)]);
    }

    public void Restore(String refer, Object args)
    {
        var func = GetFunc(refer, "Restore");
        func?.Invoke(JsValue.Undefined, [JsValue.FromObject(_engine, args)]);
    }

	public List<DeferredElement>? GetDeferred()
	{
        return _deferredTarget?.Deferred;
	}

    public Object? GetLastResult()
    {
        return _engine.GetValue("LastResult").ToObject();
    }

    public ExpandoObject? GetResult()
    {
        var func = GetFunc(_root.Id, "Result");
        return func?.Invoke(JsValue.Undefined, null).ToObject() as ExpandoObject;
    }

    public T? Evaluate<T>(String refer, String name)
    {
        _deferredTarget.Refer = refer;
        //_tracker.Track(new ScriptTrackRecord(ScriptTrackAction.Evaluate, refer, name));
        var func = GetFunc(refer, name);
        T? res = default;
        if (func == null)
            res = default;
        else
        {
            var obj = func(JsValue.Undefined, null).ToObject();
            if (obj is T objT)
                res = objT;
            else
                res = (T?)Convert.ChangeType(obj, typeof(T));
        }
        //_tracker.Track(new ScriptTrackRecord(ScriptTrackAction.EvaluateResult, refer, name, res));
        return res;
    }

    private Func<JsValue, JsValue[]?, JsValue>? GetFunc(String refer, String name)
    {
        if (ScriptData.TryGetValue(refer, out Object? activityData))
        {
            if (activityData is IDictionary<String, Object> expData)
            {
                if (expData.TryGetValue(name, out Object? objVal))
                    return (Func<JsValue, JsValue[]?, JsValue>)objVal;
            }
        }
        return null;
    }

    public void Execute(String refer, String name)
    {
        _deferredTarget.Refer = refer;
        //_tracker.Track(new ScriptTrackRecord(ScriptTrackAction.Execute, refer, name));
        var func = GetFunc(refer, name);
        if (func == null)
            return;
        func(JsValue.Undefined, null);
    }
    public void SetLastResult(Object? result)
    {
        _engine.SetValue("LastResult", result ?? new ExpandoObject());
    }
    public void SetCurrentUser(Object? result)
    {
        _engine.SetValue("CurrentUser", result ?? new ExpandoObject());
    }

    public void ExecuteResult(String refer, String name, Object? result)
    {
        _deferredTarget.Refer = refer;
        _tracker.Track(new ScriptTrackRecord(ScriptTrackAction.ExecuteResult, refer, name, result));
        var func = GetFunc(refer, name);
        if (func == null)
            return;
        // result must be not null
        var arg = JsValue.FromObject(_engine, result ?? new ExpandoObject());
        func(JsValue.Undefined, [arg]);
    }

    public void SetVariable(String refer, String name, Object? value)
    {
        _deferredTarget.Refer = refer;
        _tracker.Track(new ScriptTrackRecord(ScriptTrackAction.SetVariable, refer, name, value));
        var func = GetFunc(refer, name);
        if (func == null)
            return;
        // result may be null
        var arg = JsValue.FromObject(_engine, value);
        func(JsValue.Undefined, [arg]);
    }

    public ExpandoObject LoadPersistentValue(ExpandoObject variable, String key)
    {
        var insStorage = _serviceProvider.GetRequiredService<IInstanceStorage>();
        var id = variable.Eval<Object>($"{key}.Id")
            ?? throw new WorkflowException($"LoadPersistentValue. Id is required. Name: '{key}'");
        var loadProcedure = $"{_root.Id}.{key}.LoadPersistent";
        return insStorage.LoadPersistentValue(loadProcedure, id);
    }

    public ExpandoObject SavePersistentValue(ExpandoObject variable, String key)
    {
        var insStorage = _serviceProvider.GetRequiredService<IInstanceStorage>();

        var _ = variable.Eval<Object>("Id")
            ?? throw new WorkflowException($"SavePersistentValue. Id is required. Name: '{key}'");
        var saveProcedure = $"{_root.Id}.{key}.SavePersistent";
        insStorage.SavePersistentValue(saveProcedure, variable);
        return variable;

    }
}

