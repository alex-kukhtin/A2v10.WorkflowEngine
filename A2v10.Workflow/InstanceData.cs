// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	public class InstanceData : IInstanceData
	{
		public ExpandoObject ExternalVariables { get; init; }
		public List<Object> ExternalBookmarks { get; init; }
		public List<Object> ExternalEvents { get; init; }
		public List<Object> TrackRecords { get; init; }
		public List<DeferredElement> Deferred { get; init; }

	}
}
