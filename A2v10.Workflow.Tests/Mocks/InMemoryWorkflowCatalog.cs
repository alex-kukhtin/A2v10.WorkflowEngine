// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Tests
{
	public class CatalogWorkflow
	{
		public String Body { get; set; }
		public String Format { get; set; }
	}

	public class InMemoryWorkflowCatalog : IWorkflowCatalog
	{
		private readonly Dictionary<String, CatalogWorkflow> _storage = new();

		public Task<WorkflowElem> LoadBodyAsync(String id)
		{
			if (!_storage.TryGetValue(id, out CatalogWorkflow wf))
				throw new KeyNotFoundException(id);
			var wfe = new WorkflowElem()
			{
				Format = wf.Format,
				Body = wf.Body
			};
			return Task.FromResult(wfe);
		}

		public Task<WorkflowThumbElem> LoadThumbAsync(string id)
		{
			throw new NotImplementedException();
		}

		public Task SaveAsync(IWorkflowDescriptor workflow)
		{
			if (_storage.ContainsKey(workflow.Id))
				_storage[workflow.Id].Body = workflow.Body;
			else
				_storage.Add(workflow.Id, new CatalogWorkflow()
				{
					Body = workflow.Body,
					Format = workflow.Format
				});
			return Task.CompletedTask;
		}
	}
}
