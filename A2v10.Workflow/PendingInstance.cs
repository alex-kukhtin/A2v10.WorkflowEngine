﻿// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	public class PendingInstance : IPendingInstance
	{
		public Guid InstanceId { get; set; }
		public string EventKey { get; set; }
	}
}