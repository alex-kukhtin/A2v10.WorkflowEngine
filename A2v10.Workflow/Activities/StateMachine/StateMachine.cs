// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;

namespace A2v10.Workflow
{
    public class StateMachine : Activity, IStorable, IScoped
    {
        public List<IVariable>? Variables { get; set; }
        public String? GlobalScript { get; set; }

        public List<StateBase>? States { get; set; }

        String? _currentState; // for debug only
        IToken? _token;

        #region IStorable
        const String CURRENT_STATE = "CurrentState";
        const String TOKEN = "Token";

        public void Store(IActivityStorage storage)
        {
            storage.Set(CURRENT_STATE, _currentState);
            storage.SetToken(TOKEN, _token);
        }
        public void Restore(IActivityStorage storage)
        {
            _currentState = storage.Get<String>(CURRENT_STATE);
            _token = storage.GetToken(TOKEN);
        }
        #endregion

        public override IEnumerable<IActivity> EnumChildren()
        {
            if (States != null)
                foreach (var state in States)
                    yield return state;
        }

        public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
        {
            var startNode = States?.Find(s => s.IsStart);
            if (startNode == null)
                throw new WorkflowException("Flowchart. Start node not found");
            _currentState = startNode.Id;
            context.Schedule(startNode, token);
            return ValueTask.CompletedTask;
        }

        public override void TryComplete(IExecutionContext context, IActivity activity)
        {
            if (activity is not StateBase stateBase)
                throw new InvalidProgramException("Invalid cast 'StateBase'");
            var nextState = States?.Find(st => st.Id == stateBase.NextState);
            if (nextState != null)
            {
                _currentState = nextState.NextState;
                context.Schedule(nextState, _token);
            }
        }


        #region IScriptable
        public virtual void BuildScript(IScriptBuilder builder)
        {
            builder.AddVariables(Variables);
        }
        #endregion

    }
}
