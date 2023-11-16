// Copyright © 2020-2022 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public interface IActivity
{
    String Id { get; init; }
    IActivity? Parent { get; }

    ValueTask ExecuteAsync(IExecutionContext context, IToken? token);
    void Cancel(IExecutionContext context);
    void TryComplete(IExecutionContext context, IActivity activity);

    IEnumerable<IActivity> EnumChildren();
    void OnEndInit(IActivity? parent);

}

