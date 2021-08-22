// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Engine
{
	public class Instance : IInstance
	{
		public IWorkflow Workflow { get; init; }

		public Guid Id { get; init; }
		public Guid? Parent { get; init; }

		public WorkflowExecutionStatus ExecutionStatus { get; set; }
		public Guid? Lock { get; init; }

		public ExpandoObject Result { get; set; }
		public ExpandoObject State { get; set; }

		public IInstanceData InstanceData { get; set; }
	}
}
