// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	public class FlowActivity : FlowNode, IStorable
	{
		public IActivity Activity { get; set; }

		ExecutingAction _onComplete;
		IToken _token;

		#region IStorable
		const String ON_COMPLETE = "OnComplete";
		const String TOKEN = "Token";

		public void Store(IActivityStorage storage)
		{
			storage.SetCallback(ON_COMPLETE, _onComplete);
			storage.SetToken(TOKEN, _token);
		}

		public void Restore(IActivityStorage storage)
		{
			_onComplete = storage.GetCallback(ON_COMPLETE);
			_token = storage.GetToken(TOKEN);
		}
		#endregion

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token, ExecutingAction onComplete)
		{
			_onComplete = onComplete;
			context.Schedule(Activity, OnChildComplete, token);
			return ValueTask.CompletedTask;
		}

		public override IEnumerable<IActivity> EnumChildren()
		{
			yield return Activity;
		}

		[StoreName("OnChildComplete")]
		ValueTask OnChildComplete(IExecutionContext context, IActivity activity)
		{
			if (Next != null)
				context.Schedule(Parent.FindNode(Next), _onComplete, _token);
			else if (_onComplete != null)
				return _onComplete(context, this);
			return ValueTask.CompletedTask;
		}
	}
}
