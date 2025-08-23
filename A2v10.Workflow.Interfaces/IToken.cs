// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public interface IToken
{
    public String ToString();
    public Boolean IsEmpty { get; }
    public void SetEmpty();
    public IToken Clone();
}

