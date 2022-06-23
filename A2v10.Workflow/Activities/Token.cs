// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow
{
    public struct Token : IToken
    {
        public Guid Id { get; init; }

        private Token(Guid guid)
        {
            Id = guid;
        }

        public static IToken Empty()
		{
            return new Token(Guid.Empty);
		}

        public static IToken Create()
        {
            return new Token(Guid.NewGuid());
        }

        public Boolean IsEmpty => Id == Guid.Empty;

        public override String ToString()
        {
            return Id.ToString();
        }

        public static IToken FromString(String str)
        {
            return new Token(Guid.Parse(str));
        }
    }
}
