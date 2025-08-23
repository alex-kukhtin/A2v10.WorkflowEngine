// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow;

public struct Token : IToken
{
    public Guid Id { get; private set; }

    private Token(Guid guid)
    {
        Id = guid;
    }

    public static IToken Empty() => new Token(Guid.Empty);
    public static IToken Create() => new Token(Guid.NewGuid());
    public IToken Clone() => new Token(this.Id);
    public Boolean IsEmpty => Id == Guid.Empty;
    public void SetEmpty() => Id = Guid.Empty;
    public override String ToString() => Id.ToString();
    public static IToken FromString(String str) => new Token(Guid.Parse(str));
}
