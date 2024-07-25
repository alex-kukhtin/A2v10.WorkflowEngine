// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

using System.Collections.Generic;
using System.Text;


namespace A2v10.Workflow;

public class ActivityScriptBuilder(IActivity activity) : IScriptBuilder
{
    public const String FMAP = "__fmap__";

    private String? _declaratons;
    private readonly Dictionary<String, List<String>> _methods = [];

    private readonly IActivity _activity = activity;

    public String Declarations => _declaratons ?? String.Empty;

    void AddMethods(String refer, List<String> methods)
    {
        if (!_methods.TryAdd(refer, methods))
            _methods[refer].AddRange(methods);
    }

    void AddMethod(String refer, String method)
    {
        if (_methods.TryGetValue(refer, out var list))
            list.Add(method);
        else
            _methods.Add(refer, [method]);
    }

    #region IScriptBuilder

    public void AddVariables(IEnumerable<IVariable>? variables)
    {
        if (variables == null)
            return;
        // declare
        var sb = new StringBuilder();
        foreach (var v in variables)
            sb.Append(
                v.Dir switch
                {
                    VariableDirection.Const => $"const {v.Name}; ",
                    _ => $"let {v.Name}{v.Assignment()}; "
                }
            );

        _declaratons = sb.ToString();

        var mtds = new List<String>();
        // arguments - In, InOut
        {
            var args = variables.Where(v => v.IsArgument).ToList();
            if (args.Count != 0)
                mtds.Add($"Arguments: (_arg_) => {{ {String.Join("; ", args.Select(x => $"{x.Name} = {x.ToType($"_arg_.{x.Name}")}"))}; }}");
        }
        // result - Out, InOut
        {
            var res = variables.Where(v => v.IsResult).ToList();
            if (res.Count != 0)
                mtds.Add($"Result: () => {{return {{ {String.Join(", ", res.Select(x => $"{x.Name} : {x.Name}"))} }}; }}");
        }
        // store, restore - In, Out, Local, not constant!
        {
            var strest = variables.Where(v => v.Dir != VariableDirection.Const).ToList();
            if (strest.Count != 0)
            {
                mtds.Add($"Store: () => {{return {{ {String.Join(", ", strest.Select(x => $"{x.Name} : {x.Name}"))} }}; }}");
                mtds.Add($"Restore: (_arg_) => {{ {String.Join("; ", strest.Select(x => $"{x.Name} = _arg_.{x.Name} "))}; }}");
            }
        }

        // mtds.Add($"SetItem: (_arg_) => {{ item = _arg_; }}");

        if (mtds.Count > 0)
        {
            AddMethods(_activity.Id, mtds);
        }
    }

    public void BuildExecute(String name, String? expression)
    {
        if (String.IsNullOrEmpty(expression))
            return;
        AddMethod(_activity.Id, $"{name}: () => {{{expression.Trim()};}}");
    }

    public void BuildExecuteResult(String name, String? expression)
    {
        if (String.IsNullOrEmpty(expression))
            return;
        AddMethod(_activity.Id, $"{name}: (reply) => {{{expression.Trim()};}}");
    }

    public void BuildEvaluate(String name, String? expression)
    {
        if (String.IsNullOrEmpty(expression))
            return;
        AddMethod(_activity.Id, $"{name}: () => {{ return {expression.Trim()};}}");
    }

    public void BuildSetVariable(String name, String? expression)
	{
        if (String.IsNullOrEmpty(expression))
            return;
        AddMethod(_activity.Id, $"{name}: (_arg_) => {{ {expression} = _arg_;}}");

	}

    #endregion

    public String Methods
    {
        get
        {
            var sb = new StringBuilder();
            static String _methodsText(List<String> methods)
            {
                if (methods == null || methods.Count == 0)
                    return "null";
                return $"{{\n{String.Join(",\n", methods)}\n}};";
            }

            foreach (var (k, v) in _methods)
                sb.Append($"{FMAP}['{k}'] = {_methodsText(v)}");
            return sb.ToString();
        }
    }
}

public class ActivityScript(ScriptBuilder builder, IActivity activity, String? global)
{
    private readonly ScriptBuilder _builder = builder;
    private readonly IActivity _activity = activity;

    public String? Global { get; private set; } = global;

    public String Ref => _activity.Id ?? throw new WorkflowException("Invalid activity Id");

    public void ClearGlobal() => Global = null; 
    public void Build(IActivity activity)
    {
        if (activity is IScriptable scriptable)
        {
            var scriptBuilder = new ActivityScriptBuilder(activity);
            scriptable.BuildScript(scriptBuilder);

            _builder.Append(scriptBuilder.Declarations);
            _builder.Global();
            _builder.Append(scriptBuilder.Methods);
        }
    }
}

public class ScriptBuilder
{
    private readonly Stack<ActivityScript> _stack = new();
    private readonly StringBuilder _textBuilder = new();

    public ScriptBuilder()
    {
        _textBuilder.AppendLine("\"use strict\";");
        _textBuilder.AppendLine("() => {");
        _textBuilder.AppendLine($"let {ActivityScriptBuilder.FMAP} = {{}};");
    }

    public void EndScript()
    {
        _textBuilder.AppendLine($"return {ActivityScriptBuilder.FMAP};");
        _textBuilder.AppendLine("};");
    }

    public String Script => _textBuilder.ToString();

    public void Start(IActivity activity)
    {
        if (activity is IScoped scopedActivity)
        {
            var globalScript = scopedActivity.GlobalScript;
            var ascript = new ActivityScript(this, activity, globalScript);
            _stack.Push(ascript);
            _textBuilder.AppendLine("(function() {");
        }
    }

    public void Global()
    {
        var ascript = _stack.Peek();
        if (String.IsNullOrEmpty(ascript.Global))
            return;
        _textBuilder.AppendLine(ascript.Global);
        ascript.ClearGlobal();
    }

    public void Append(String text)
    {
        if (!String.IsNullOrEmpty(text))
            _textBuilder.AppendLine(text);
    }

    public void End(IActivity activity)
    {
        var ascript = _stack.Peek();
        if (ascript.Ref == activity.Id)
        {
            _textBuilder.AppendLine("})();");
            _stack.Pop();
        }
    }

    public void Build(IActivity activity)
    {
        var ascript = _stack.Peek();
        ascript.Build(activity);
    }
}
