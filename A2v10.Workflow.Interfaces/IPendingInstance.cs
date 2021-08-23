// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Workflow.Interfaces
{
	public interface IPendingInstance
	{
		Guid InstanceId { get; }
		String EventKey { get; }
	}
}
