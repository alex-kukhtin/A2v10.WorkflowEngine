// Copyright © 2021-2022 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow;

public static class StringExtensions
{
    public static Boolean IsVariable(this String? expression)
    {
        if (String.IsNullOrEmpty(expression))
            return false;
        var ex = expression.Trim();
        return ex.StartsWith("${") && ex.EndsWith("}");
    }

    public static String Variable(this String? expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));
        var exp = expression.Trim();
        return exp[2..^1];
    }
}
