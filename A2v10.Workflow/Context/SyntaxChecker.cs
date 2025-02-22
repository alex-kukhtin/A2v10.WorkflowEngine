
// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using System.Collections.Generic;

using Acornima;

namespace A2v10.Workflow;

public record SyntaxError(String Script, String Message, String? ActivityId = null) : ISyntaxError;
public class SyntaxChecker
{
    public static IEnumerable<ISyntaxError> CheckSyntax(IActivity root)
    {
        var sb = new ScriptBuilder(emptyBuilder: true);

        List<SyntaxError> errors = [];
        var sbTraverseArg = new TraverseArg()
        {
            Action = (activity) => {
                foreach (var s in sb.CheckSyntax(activity))
                {
                    if (s != null)
                        errors.Add(new SyntaxError(s.Script, s.Message, activity.Id));
                }
            },
        };
        root.Traverse(sbTraverseArg);
        return errors;
    }

    public static SyntaxError? CheckSyntax(String script)
    {
        if (String.IsNullOrEmpty(script))
            return null;
        try
        {
            var parser = new Acornima.Parser();
            parser.ParseScript(script);
        }
        catch (SyntaxErrorException ex)
        {
            return new SyntaxError(script, ex.Message);
        }
        return null;
    }
}
