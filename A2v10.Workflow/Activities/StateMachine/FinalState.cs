// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;

namespace A2v10.Workflow
{
    public class FinalState : StateBase
    {
        public override Boolean IsFinal => false;

        public IActivity? Entry { get; set; }

        public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
        {
            if (Entry != null)
                context.Schedule(Entry, token);
            return ValueTask.CompletedTask;
        }

        public override IEnumerable<IActivity> EnumChildren()
        {
            if (Entry != null)
                yield return Entry;
        }
    }
}
