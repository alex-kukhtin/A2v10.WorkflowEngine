// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow
{
    public class FlowStart : FlowNode
    {
        public override bool IsStart => true;

        public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
        {
            var node = ParentFlow.FindNode(Next);
            if (node != null)
                context.Schedule(node, token);
            return ValueTask.CompletedTask;
        }
    }
}
