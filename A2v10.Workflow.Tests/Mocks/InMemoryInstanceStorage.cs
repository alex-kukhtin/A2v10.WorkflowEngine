// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Tests;

internal record SavedInstance(IWorkflowIdentity Identity, IWorkflow Workflow,
    String? State, IInstanceData? InstanceData, ExpandoObject? Result, WorkflowExecutionStatus Status, Guid? Parent)
{
    public Boolean IsEmptyWorkflow => Identity == null || String.IsNullOrEmpty(Identity.Id) && Identity.Version == 0;
    public Boolean HasWorkflow => !IsEmptyWorkflow;
}


public class InMemoryInstanceStorage(ISerializer serializer, IWorkflowStorage workflowStorage) : IInstanceStorage
{
    private readonly Dictionary<Guid, SavedInstance> _memory = [];

    private readonly ISerializer _serializer = serializer;
    private readonly IWorkflowStorage _workflowStorage = workflowStorage;

    public Task<IInstance> LoadRaw(Guid id)
    {
        return Load(id);
    }

    public async Task<IInstance> Load(Guid id)
    {
        if (_memory.TryGetValue(id, out SavedInstance? saved))
        {
            var wf = saved.HasWorkflow ? await _workflowStorage.LoadAsync(saved.Identity) : saved.Workflow;
            IInstance inst = new Instance(wf, id)
            {
                State = _serializer.Deserialize(saved.State),
                Result = saved.Result,
                ExecutionStatus = saved.Status,
                Parent = saved.Parent
            };
            return inst;
        }
        throw new KeyNotFoundException();
    }

    public Task Create(IInstance instance)
    {
        if (_memory.ContainsKey(instance.Id))
            throw new WorkflowException($"Instance storage. Instance with id = {instance.Id} has been already created");
        var si = new SavedInstance(instance.Workflow.Identity, instance.Workflow,
            _serializer.Serialize(instance.State), instance.InstanceData,
            instance.Result, instance.ExecutionStatus, instance.Parent);
        _memory.Add(instance.Id, si);
        return Task.CompletedTask;
    }

    public Task Save(IInstance instance)
    {
        if (_memory.ContainsKey(instance.Id))
        {
            var si = new SavedInstance(instance.Workflow.Identity, instance.Workflow,
                _serializer.Serialize(instance.State), instance.InstanceData,
                instance.Result, instance.ExecutionStatus, instance.Parent);
            _memory[instance.Id] = si;
        }
        else
            throw new WorkflowException($"Instance storage. Instance with id = {instance.Id} not found");
        return Task.CompletedTask;
    }

    public Task WriteException(Guid id, Exception ex)
    {
        Debug.WriteLine($"id: {id}, ex: {ex.Message}");
        return Task.CompletedTask;
    }

    private static String? IsTimerExpired(Object ev)
    {
        if (ev == null)
            return null;
        var now = DateTime.UtcNow; // + TimeSpan.FromSeconds(2); // for testing propouses
        if (ev is ExpandoObject eo)
        {
            var kind = eo.Get<String>("Kind");
            if (kind == "T")
            {
                var exp = eo.Get<DateTime>("Pending");
                if (exp <= now)
                    return eo.Get<String>("Event");
            }
        }
        return null;
    }

    public Task<PendingElement?> GetPendingAsync()
    {
        // timers
        var pendingList = new List<IPendingInstance>();
        foreach (var (k, v) in _memory)
        {
            var extEvents = v.InstanceData?.ExternalEvents;
            if (extEvents != null)
            {
                var pendDict = new Dictionary<Guid, PendingInstance>();
                foreach (var ev in extEvents)
                {
                    var eventKey = IsTimerExpired(ev);
                    if (String.IsNullOrEmpty(eventKey))
                        continue;
                    if (!pendDict.TryGetValue(k, out PendingInstance? pendingInstance))
                    {
                        pendingInstance = new PendingInstance() { InstanceId = k };
                        pendDict.Add(k, pendingInstance);

                    }
                    pendingInstance.AddEventKey(eventKey);
                }
                foreach (var pi in pendDict.Values)
                    pendingList.Add(pi);
            }
        }
        var autoStartList = new List<IAutoStartInstance>();
        var res = new PendingElement(Pending: pendingList, AutoStart: autoStartList);
        return Task.FromResult<PendingElement?>(res);
    }

    public Task AutoStartComplete(Int64 Id, Guid instanceId)
    {
        return Task.CompletedTask;
    }

    public async Task<IInstance?> LoadBookmark(String bookmark)
    {
        foreach (var (id, saved) in _memory)
        {
            var eb = saved?.InstanceData?.ExternalBookmarks;
            if (eb != null)
            {
                foreach (var bi in eb)
                {
                    if (bi is ExpandoObject eo)
                    {
                        if (eo.Get<String>("Bookmark") == bookmark)
                            return await Load(id);
                    }
                }
            }
        }
        return null;
    }
    public ValueTask<DateTime> GetNowTime()
    {
        return ValueTask.FromResult<DateTime>(DateTime.UtcNow);
    }

    public ExpandoObject LoadPersistentValue(string procedure, object id)
    {
        throw new NotImplementedException();
    }

    public ExpandoObject SavePersistentValue(String procedure, ExpandoObject obj)
    {
        throw new NotImplementedException();
    }

    public Task SetPersistentInstanceAsync(String procedure, String correlationId, Guid instanceId)
    {
        throw new NotImplementedException();
    }

    public Task CancelChildren(Guid id, String workflow)
    {
        return Task.CompletedTask;
    }
}
