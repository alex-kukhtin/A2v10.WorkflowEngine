// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	public class Wait : ActivityWithComplete
	{
		public String Bookmark { get; set; }

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token, ExecutingAction onComplete)
		{
			_onComplete = onComplete;
			context.SetBookmark(Bookmark, this, OnBookmarkComplete);
			return ValueTask.CompletedTask;
		}

		[StoreName("OnBookmarkComplete")]
		ValueTask OnBookmarkComplete(IExecutionContext context, String bookmark, Object result)
		{
			context.RemoveBookmark(bookmark);
			if (_onComplete != null)
				return _onComplete(context, this);
			return ValueTask.CompletedTask;
		}

	}
}
