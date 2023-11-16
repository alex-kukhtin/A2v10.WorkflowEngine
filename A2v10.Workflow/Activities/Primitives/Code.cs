// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow
{
    public class Code : Activity, IScriptable
    {
        public String? Script { get; set; }

        public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
        {
            if (!String.IsNullOrEmpty(Script))
                context.Execute(Ref, nameof(Script));
            Parent?.TryComplete(context, this);
            return ValueTask.CompletedTask;
        }

        #region IScriptable
        public void BuildScript(IScriptBuilder builder)
        {
            builder.BuildExecute(nameof(Script), Script);
        }
        #endregion
    }
}
