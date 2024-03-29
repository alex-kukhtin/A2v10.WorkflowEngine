﻿// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using System.Collections.Generic;

namespace A2v10.Workflow
{
    public class If : Activity, IScriptable
    {
        public String? Condition { get; set; }

        public IActivity? Then { get; set; }
        public IActivity? Else { get; set; }

        public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
        {
            var cond = context.Evaluate<Boolean>(Ref, nameof(Condition));
            if (cond)
            {
                if (Then != null)
                    context.Schedule(Then, token);
            }
            else
            {
                if (Else != null)
                    context.Schedule(Else, token);
            }
            return ValueTask.CompletedTask;
        }

        public override IEnumerable<IActivity> EnumChildren()
        {
            if (Then != null)
                yield return Then;
            if (Else != null)
                yield return Else;
        }

        #region IScriptable
        public void BuildScript(IScriptBuilder builder)
        {
            builder.BuildEvaluate(nameof(Condition), Condition);
        }
        #endregion
    }
}
