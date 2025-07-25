// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.


using A2v10.System.Xaml;

namespace A2v10.Workflow.Bpmn;

[ContentProperty("Children")]
public class Process : ProcessBase
{
    public Boolean IsExecutable { get; init; }
    public Boolean IsClosed { get; init; }

    public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
    {
        _token = token;
        if (!IsExecutable || Children == null)
            return ValueTask.CompletedTask;
        var start = Elems<Event>().FirstOrDefault(ev => ev.IsStart) 
            ?? throw new WorkflowException($"Process (Id={Id}). Start event not found");
        context.Schedule(start, _token);
        return ValueTask.CompletedTask;
    }

    public override void TryComplete(IExecutionContext context, IActivity _)
    {
        if (TokensCount > 0)
            return;
    }
}

