// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;

namespace A2v10.Workflow.Interfaces
{
	public enum DeferredElementType
	{
		Sql
	}

	public record DeferredElement(DeferredElementType Type, String Name, ExpandoObject Parameters, String Refer);

	public interface IInstanceData
	{
		ExpandoObject ExternalVariables { get; }
		List<Object> ExternalBookmarks { get; }
		List<Object> TrackRecords { get; }
		List<Object> ExternalEvents { get; }

		List<DeferredElement> Deferred { get; }
	}
}
