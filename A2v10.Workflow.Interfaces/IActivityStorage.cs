// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;

public interface IActivityStorage
{
    Boolean IsLoading { get; }
    Boolean IsStoring { get; }

    void Set<T>(String name, T value);
    void SetToken(String name, IToken? value);
    void SetTokenList(String name, List<IToken> list);

    T? Get<T>(String name);
    IToken? GetToken(String name);
    void GetTokenList(String name, List<IToken> list);
}

