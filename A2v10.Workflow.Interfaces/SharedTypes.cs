// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces
{
	public enum ActivityTrackAction
	{
		// see: db
		Schedule = 0,
		Execute = 1,
		Bookmark = 2,
		Resume = 3,
		Event = 4,
		HandleEvent = 5,
		Exception = 999
	}

	public enum TrackRecordKind
	{
		// see: db
		Activity = 0,
		Script = 1,
		Storage = 99
	}
}
