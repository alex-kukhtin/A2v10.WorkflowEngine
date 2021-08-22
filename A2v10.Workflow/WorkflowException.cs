// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Workflow
{
	public sealed class WorkflowException : Exception
	{
		public WorkflowException(String message)
			: base(message)
		{

		}
	}
}
