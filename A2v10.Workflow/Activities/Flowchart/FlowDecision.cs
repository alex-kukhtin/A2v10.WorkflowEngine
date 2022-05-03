// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow
{
    public class FlowDecision : FlowNode, IScriptable
    {
        public String? Condition { get; set; }
        public String? Then { get; set; }
        public String? Else { get; set; }

        public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
        {
            var cond = context.Evaluate<Boolean>(Ref, nameof(Condition));
            var nextNode = ParentFlow.FindNode(cond ? Then : Else);
            if (nextNode == null)
                nextNode = ParentFlow.FindNode(Next);
            if (nextNode != null)
                context.Schedule(nextNode, token);
            else
                Parent?.TryComplete(context, this);
            return ValueTask.CompletedTask;
        }

        #region IScriptable
        public void BuildScript(IScriptBuilder builder)
        {
            builder.BuildEvaluate(nameof(Condition), Condition);
        }
        #endregion
    }
}
