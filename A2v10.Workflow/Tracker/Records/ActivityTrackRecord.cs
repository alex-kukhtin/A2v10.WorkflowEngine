// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Tracker
{
	public class ActivityTrackRecord : TrackRecord
	{
		private readonly ActivityTrackAction _action;
		private readonly String _id;

		public ActivityTrackRecord(ActivityTrackAction action, IActivity activity, IToken token)
			: base()
		{
			_action = action;
			_id = activity.Id;
			Message = token != null ? $"{{token:'{token}'}}" : null;
		}

		public ActivityTrackRecord(ActivityTrackAction action, IActivity activity, String msg)
		{
			_action = action;
			_id = activity?.Id;
			Message = msg;
		}

		public override ExpandoObject ToExpandoObject(int no)
		{
			var eo = CreateExpando(no);
			eo.Set("Kind", (Int32)TrackRecordKind.Activity);
			eo.Set("Action", (Int32)_action);
			eo.Set("Activity", _id);
			return eo;
		}

		public override string ToString()
		{
			return $"Activity.{_action}: id: {_id}, msg: {Message}";
		}
	}
}
