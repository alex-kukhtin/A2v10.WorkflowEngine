// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;

namespace A2v10.Workflow
{
    public class FlowActivity : FlowNode, IStorable
    {
        public IActivity? Activity { get; set; }

        IToken? _token;

        #region IStorable
        const String TOKEN = "Token";

        public void Store(IActivityStorage storage)
        {
            storage.SetToken(TOKEN, _token);
        }

        public void Restore(IActivityStorage storage)
        {
            _token = storage.GetToken(TOKEN);
        }
        #endregion

        public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
        {
            if (Activity == null)
                return ValueTask.CompletedTask;
            context.Schedule(Activity, token);
            return ValueTask.CompletedTask;
        }

        public override IEnumerable<IActivity> EnumChildren()
        {
            if (Activity != null)
                yield return Activity;
        }

        public override void TryComplete(IExecutionContext context, IActivity activity)
        {
            if (Next != null)
            {
                var nextNode = ParentFlow.FindNode(Next);
                if (nextNode == null)
                    throw new InvalidOperationException($"Node '{Next}' not found");
                context.Schedule(nextNode, _token);
            }
            else
                Parent?.TryComplete(context, activity);
        }
    }
}
