// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow;
public class Wait : Activity
{
    public String? Bookmark { get; set; }

    public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
    {
        if (Bookmark == null)
            throw new InvalidProgramException("Bookmark is null");
        context.SetBookmark(Bookmark, this, OnBookmarkComplete);
        return ValueTask.CompletedTask;
    }

    [StoreName("OnBookmarkComplete")]
    ValueTask OnBookmarkComplete(IExecutionContext context, String bookmark, Object? result)
    {
        context.RemoveBookmark(bookmark);
        Parent?.TryComplete(context, this);
        return ValueTask.CompletedTask;
    }

}

