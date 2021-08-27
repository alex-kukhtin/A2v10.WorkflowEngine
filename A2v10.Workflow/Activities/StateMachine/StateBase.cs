// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	public abstract class StateBase : Activity
	{
		public virtual Boolean IsStart => false;
		public virtual Boolean IsFinal => false;

		public String Next { get; set; }

		internal String NextState { get; set; }
	}
}
