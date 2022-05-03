// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Bpmn
{
    public class IntermediateThrowEvent : Event
    {
        public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
        {
            throw new NotImplementedException();
        }
    }
}
