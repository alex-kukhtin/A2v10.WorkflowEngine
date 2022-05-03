// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;

namespace A2v10.Workflow
{
    public class Sequence : Activity, IStorable, IScoped
    {
        public List<IActivity>? Activities { get; set; }
        public List<IVariable>? Variables { get; set; }
        public String? GlobalScript { get; set; }

        Int32 _next;
        IToken? _token;

        #region IStorable
        const String NEXT = "Next";
        const String TOKEN = "Token";

        public void Store(IActivityStorage storage)
        {
            storage.Set<Int32>(NEXT, _next);
            storage.SetToken(TOKEN, _token);
        }

        public void Restore(IActivityStorage storage)
        {
            _next = storage.Get<Int32>(NEXT);
            _token = storage.GetToken(TOKEN);
        }
        #endregion

        #region IScriptable
        public virtual void BuildScript(IScriptBuilder builder)
        {
            builder.AddVariables(Variables);
        }
        #endregion


        public override IEnumerable<IActivity> EnumChildren()
        {
            if (Activities != null)
            {
                foreach (var a in Activities)
                    yield return a;
            }
        }

        public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
        {
            _token = token;
            if (Activities == null || Activities.Count == 0)
            {
                Parent?.TryComplete(context, this);
                return ValueTask.CompletedTask;
            }
            _next = 0;
            var first = Activities[_next++];
            context.Schedule(first, token);
            return ValueTask.CompletedTask;
        }

        public override void TryComplete(IExecutionContext context, IActivity activity)
        {
            if (Activities != null && _next < Activities.Count)
            {
                var next = Activities[_next++];
                context.Schedule(next, _token);
            }
            else
                Parent?.TryComplete(context, this);
        }
    }
}
