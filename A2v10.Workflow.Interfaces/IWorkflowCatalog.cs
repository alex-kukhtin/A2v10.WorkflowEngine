// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;

namespace A2v10.Workflow.Interfaces
{
	public interface IWorkflowDescriptor
	{
		String Id { get; }
		String Format { get; }
		String Body { get; }

		String ThumbFormat { get; }
		Stream Thumb { get; }
	}

	public record WorkflowDescriptor : IWorkflowDescriptor
	{
		public String Id { get; init; }
		public String Format { get; init; }
		public String Body { get; init; }

		public String ThumbFormat { get; init; }
		public Stream Thumb { get; init; }
	}

	public record WorkflowElem
	{
		public String Body { get; init; }
		public String Format { get; init; }
	}

	public record WorkflowThumbElem
	{
		public Stream Thumb { get; init; }
		public String Format { get; init; }
	}

	public interface IWorkflowCatalog
	{
		Task<WorkflowElem> LoadBodyAsync(String id);
		Task<WorkflowThumbElem> LoadThumbAsync(String id);
		Task SaveAsync(IWorkflowDescriptor workflow);
	}
}
