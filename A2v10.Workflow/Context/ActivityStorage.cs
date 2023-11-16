// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

using System.Collections.Generic;
using System.Dynamic;

namespace A2v10.Workflow;

public enum StorageState
{
    Loading,
    Storing
}

public class ActivityStorage(StorageState state, ExpandoObject? obj = null) : IActivityStorage
{
    private readonly ExpandoObject _expando = obj ?? [];

    public Boolean IsLoading { get; set; } = state == StorageState.Loading;
    public Boolean IsStoring => !IsLoading;

    public ExpandoObject Value => _expando;

    public T? Get<T>(String name)
    {
        if (IsStoring)
            throw new InvalidOperationException("Get in storing mode");
        return _expando.Get<T>(name);
    }


    public void Set<T>(String name, T value)
    {
        if (IsLoading)
            throw new InvalidOperationException("Set in loading mode");
        _expando.Set(name, value);
    }

    public void SetToken(String name, IToken? value)
    {
        if (IsLoading)
            throw new InvalidOperationException("Set in loading mode");
        if (value == null)
            return;
        _expando.Set(name, value.ToString());
    }

    public IToken? GetToken(String name)
    {
        if (IsStoring)
            throw new InvalidOperationException("Get in storing mode");
        var val = _expando.Get<String>(name);
        if (val == null)
            return null;
        return Token.FromString(val);
    }

    public void SetTokenList(String name, List<IToken> list)
    {
        if (IsLoading)
            throw new InvalidOperationException("Set in loading mode");
        if (list == null || list.Count == 0)
            return;
        var vals = new List<String>();
        foreach (var l in list)
            vals.Add(l.ToString());
        _expando.Set(name, vals);
    }

    public void GetTokenList(String name, List<IToken> list)
    {
        if (IsStoring)
            throw new InvalidOperationException("Get in storing mode");
        var vals = _expando.Get<Object[]>(name);
        if (vals == null)
            return;
        foreach (var v in vals)
        {
            list.Add(Token.FromString(v.ToString()!));
        }
    }
}
