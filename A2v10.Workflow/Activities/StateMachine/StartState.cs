// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow
{
    public class StartState : StateBase
    {
        public override Boolean IsStart => true;

        public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
        {
            NextState = Next;
            Parent?.TryComplete(context, this);
            return ValueTask.CompletedTask;
        }
    }
}
