// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.Workflow;
public readonly record struct TraverseArg(Action<IActivity>? Start, Action<IActivity>? Action, Action<IActivity>? End);

public static class ActivityExtensions
{
    public static void Traverse(this IActivity activity, TraverseArg traverse)
    {
        traverse.Start?.Invoke(activity);
        traverse.Action?.Invoke(activity);
        foreach (var ch in activity.EnumChildren())
            ch.Traverse(traverse);
        traverse.End?.Invoke(activity);
    }
}

