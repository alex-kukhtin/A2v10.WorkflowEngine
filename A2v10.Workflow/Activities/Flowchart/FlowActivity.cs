// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	public class FlowActivity : FlowNode, IStorable
	{
		public IActivity Activity { get; set; }

		IToken _token;

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

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token)
		{
			context.Schedule(Activity, token);
			return ValueTask.CompletedTask;
		}

		public override IEnumerable<IActivity> EnumChildren()
		{
			yield return Activity;
		}

		public override void TryComplete(IExecutionContext context, IActivity activity)
		{
			if (Next != null)
				context.Schedule(ParentFlow.FindNode(Next), _token);
			else
				Parent?.TryComplete(context, activity);
		}
	}
}
