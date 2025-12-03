// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

using System.Text.RegularExpressions;

namespace A2v10.Workflow.Interfaces;

public static partial class ExpandoObjectExtensions
{
    public static T GetNotNull<T>(this ExpandoObject expobj, String name)
    {
        var d = expobj as IDictionary<String, Object?>;
        if (d.TryGetValue(name, out Object? res))
        {
            if (res is T t)
                return t;
            return (T)(Convert.ChangeType(res, typeof(T)) ??
                throw new InvalidOperationException($"ChangeType failed for '{name}'"));
        }
        throw new InvalidOperationException($"{name} not found");
    }

    public static T? Get<T>(this ExpandoObject? expobj, String name)
    {
        if (expobj == null)
            return default;
        var d = expobj as IDictionary<String, Object?>;
        if (d.TryGetValue(name, out Object? res))
        {
            if (res == null)
                return default;
            if (res is T t)
                return t;
            return (T?)Convert.ChangeType(res, typeof(T));
        }
        return default;
    }

    public static void Add(this ExpandoObject expobj, String name, Object? value)
    {
        var d = expobj as IDictionary<String, Object?>;
        d.Add(name, value);
    }

    public static ExpandoObject Clone(this ExpandoObject? expobj)
    {
        var rv = new ExpandoObject();
        if (expobj == null)
            return rv;
        foreach (var (k, v) in expobj)
        {
            if (v is ExpandoObject vexp)
                rv.Add(k, vexp.Clone());
            else
                rv.Add(k, v);
        }
        return rv;
    }

    public static void Set<T>(this ExpandoObject expobj, String name, T value)
    {
        var d = expobj as IDictionary<String, Object?>;
        d.Add(name, value);
    }

    public static Boolean IsEmpty(this ExpandoObject expobj)
    {
        return expobj == null || (expobj as IDictionary<String, Object?>).Count == 0;
    }

    public static Boolean IsNotEmpty(this ExpandoObject expobj)
    {
        return expobj != null && (expobj as IDictionary<String, Object?>).Count > 0;
    }

    public static void SetNotNull<T>(this ExpandoObject expobj, String name, T? value) where T : class
    {
        if (value == null)
            return;
        var d = expobj as IDictionary<String, Object?>;
        d.Add(name, value);
    }

    public static void SetOrReplace<T>(this ExpandoObject expobj, String name, T value)
    {
        var d = expobj as IDictionary<String, Object?>;
        if (d.ContainsKey(name))
            d[name] = value;
        else
            d.Add(name, value);
    }

    public static IEnumerable<String> Keys(this ExpandoObject expobj)
    {
        return (expobj as IDictionary<String, Object>).Keys;
    }

    public static T? Eval<T>(this ExpandoObject root, String expression, T? fallback = default, Boolean throwIfError = false)
    {
        if (expression == null)
            return fallback;
        Object? result = root.EvalExpression(expression, throwIfError);
        if (result == null)
            return fallback;
        if (result is T t)
            return t;
        return fallback;
    }

    private static Object? EvalExpression(this ExpandoObject root, String expression, Boolean throwIfError = false)
    {
        Object currentContext = root;
        foreach (var exp in expression.Split('.'))
        {
            if (currentContext == null)
                return null;
            String prop = exp.Trim();
            var d = currentContext as IDictionary<String, Object>;
            if (prop.Contains('['))
            {
                var match = ArrayPattern().Match(prop);
                prop = match.Groups[1].Value;
                if ((d != null) && d.TryGetValue(prop, out Object? value))
                {
                    if (value is IList<ExpandoObject> dList)
                        currentContext = dList[Int32.Parse(match.Groups[2].Value)];
                }
                else
                {
                    if (throwIfError)
                        throw new ArgumentException($"Error in expression '{expression}'. Property '{prop}' not found");
                    return null;
                }
            }
            else
            {
                if ((d != null) && d.TryGetValue(prop, out Object? value))
                    currentContext = value;
                else
                {
                    if (throwIfError)
                        throw new ArgumentException($"Error in expression '{expression}'. Property '{prop}' not found");
                    return null;
                }
            }
        }
        return currentContext;
    }

    [GeneratedRegex(@"(\w+)\[(\d+)\]{1}")]
    private static partial Regex ArrayPattern();
}

