// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Bpmn
{
    public class IntermediateCatchEvent : Event, IStorable
    {
        protected IToken? _token;

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
            _token = token;
            var eventDef = EventDefinition;
            if (eventDef != null)
                context.AddEvent(eventDef.CreateEvent(Id, context), this, OnTrigger);
            else
                SetComplete(context);
            return ValueTask.CompletedTask;
        }

        [StoreName("OnTrigger")]
        public ValueTask OnTrigger(IExecutionContext context, IWorkflowEvent wfEvent, Object? result)
        {
            SetComplete(context);
            ScheduleOutgoing(context, _token);
            return ValueTask.CompletedTask;
        }
    }
}
