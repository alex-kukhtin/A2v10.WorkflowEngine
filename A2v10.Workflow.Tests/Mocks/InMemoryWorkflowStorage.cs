// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace A2v10.Workflow.Tests;
public record StoredWorkflow
{
    public String Text { get; set; } = String.Empty;
    public String Format { get; set; } = String.Empty;
    public String WorkflowId { get; set; } = String.Empty;
    public Int32 Version { get; set; }
}

public class InMemoryWorkflowStorage(ISerializer serializer) : IWorkflowStorage
{
    private readonly List<StoredWorkflow> _storage = [];
    private readonly ISerializer _serializer = serializer;

    public Task<String> LoadSourceAsync(IWorkflowIdentity identity)
    {
        return Task.FromResult<String>(String.Empty);
    }

    public Task<IWorkflow> LoadAsync(IWorkflowIdentity identity)
    {
        Int32 v = identity.Version;
        if (v == 0)
        {
            // find max version
            v = _storage.FindAll(sw => sw.WorkflowId == identity.Id).Max(x => x.Version);
        }
        var swf = _storage.Find(x => x.WorkflowId == identity.Id && x.Version == v) 
            ?? throw new KeyNotFoundException($"Workflow '{identity}' not found");
        var root = _serializer.DeserializeActitity(swf.Text, swf.Format);
        var wf = new WorkflowElement(
            new WorkflowIdentity(identity.Id, v),
            root
        );
        return Task.FromResult<IWorkflow>(wf);
    }

    public IActivity LoadFromBody(String body, String format)
    {
        var result = _serializer.DeserializeActitity(body, format)
            ?? throw new InvalidOperationException("LoadFromBody failed");
        return result.Activity;
    }

    public async Task<IWorkflowIdentity> PublishAsync(IWorkflowCatalog catalog, String id)
    {
        var elem = await catalog.LoadBodyAsync(id);
        return await PublishAsync(id, elem.Body, elem.Format);
    }

    private Task<IWorkflowIdentity> PublishAsync(String id, String text, String format)
    {
        // find max version
        var v = 0;
        if (_storage.Count == 0)
            v = 1;
        else
        {
            var all = _storage.FindAll(sw => sw.WorkflowId == id);
            if (all.Count == 0)
                v = 1;
            else
                v = all.Max(x => x.Version) + 1;
        }
        StoredWorkflow swf = new()
        {
            WorkflowId = id,
            Version = v,
            Text = text,
            Format = format
        };
        _storage.Add(swf);
        var ident = new WorkflowIdentity(id, v);
        return Task.FromResult<IWorkflowIdentity>(ident);
    }
}

