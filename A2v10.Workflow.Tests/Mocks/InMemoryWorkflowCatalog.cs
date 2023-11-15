// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace A2v10.Workflow.Tests
{
    public record CatalogWorkflow
    {
        public CatalogWorkflow(String body, String format)
        {
            Body = body;
            Format = format;
        }
        public String Body { get; set; }
        public String Format { get; }
    }

    public class InMemoryWorkflowCatalog : IWorkflowCatalog
    {
        private readonly Dictionary<String, CatalogWorkflow> _storage = [];

        public Task<WorkflowElem> LoadBodyAsync(String id)
        {
            if (!_storage.TryGetValue(id, out CatalogWorkflow? wf))
                throw new KeyNotFoundException(id);
            var wfe = new WorkflowElem(Body: wf.Body, Format: wf.Format);
            return Task.FromResult(wfe);
        }

        public Task<WorkflowThumbElem> LoadThumbAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task SaveAsync(IWorkflowDescriptor workflow)
        {
            if (_storage.TryGetValue(workflow.Id, out var catWorkflow))
                catWorkflow.Body = workflow.Body;
            else
                _storage.Add(workflow.Id, new CatalogWorkflow(body: workflow.Body, format: workflow.Format));
            return Task.CompletedTask;
        }
    }
}
