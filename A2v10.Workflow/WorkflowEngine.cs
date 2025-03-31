// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;

namespace A2v10.Workflow;
public class WorkflowEngine : IWorkflowEngine
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkflowStorage _workflowStorage;
    private readonly IInstanceStorage _instanceStorage;
    private readonly ITracker _tracker;
    private readonly ILogger<WorkflowEngine> _logger;

    public WorkflowEngine(IServiceProvider serviceProvider, ITracker tracker, ILogger<WorkflowEngine> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _workflowStorage = _serviceProvider.GetRequiredService<IWorkflowStorage>();
        _instanceStorage = _serviceProvider.GetRequiredService<IInstanceStorage>();
        _tracker = tracker;
        _logger = logger;
    }

    public async ValueTask<IInstance> CreateAsync(IActivity root, IWorkflowIdentity? identity, String? correlationId = null, Guid? parent = null, Guid? instanceId = null)
    {
        Guid instId = instanceId != null ? instanceId.Value : Guid.NewGuid();
        var wf = new WorkflowElement(identity ?? new WorkflowIdentity(String.Empty), root, new DymmyActivityWrapper());
        var inst = new Instance(wf, instId, correlationId, parent);
        root.OnEndInit(null);
        await _instanceStorage.Create(inst);
        return inst;
    }

    public async ValueTask<IInstance> CreateAsync(IWorkflow workflow, String? correlationId = null, Guid? parent = null, Guid? instanceId = null)
    {
        Guid instId = instanceId != null ? instanceId.Value : Guid.NewGuid();
        var inst = new Instance(workflow, instId, correlationId, parent);
        workflow.Root.OnEndInit(null);
        await _instanceStorage.Create(inst);
        return inst;
    }

    public async ValueTask<IInstance> CreateAsync(IWorkflowIdentity identity, String? correlationId = null, Guid? parent = null, Guid? instanceId = null)
    {
        var wf = await _workflowStorage.LoadAsync(identity);
        return await CreateAsync(wf, correlationId, parent, instanceId);
    }

    public async ValueTask<IInstance> RunAsync(IInstance instance, Object? args = null)
    {
        if (instance.ExecutionStatus != WorkflowExecutionStatus.Init)
            throw new WorkflowException($"Instance (id={instance.Id}) is already running");
        var context = new ExecutionContext(_serviceProvider, _tracker, instance, args);
        context.Schedule(instance.Workflow.Root, null);
        await context.RunAsync();
        SetInstanceState(instance, context);
        await _instanceStorage.Save(instance);
        await CheckParent(instance);
        return instance;
    }

    public async ValueTask<IInstance> RunAsync(Guid id, Object? args = null)
    {
        try
        {
            IInstance instance = await _instanceStorage.Load(id);
            return await RunAsync(instance, args);
        }
        catch (Exception ex)
        {
            await _instanceStorage.WriteException(id, ex);
            throw;
        }
    }

    public ValueTask<IInstance> ResumeAsync(Guid id, String bookmark, Object? reply = null)
    {
        return Handle(id, context => context.ResumeAsync(bookmark, reply));
    }

    public ValueTask<IInstance> HandleEventsAsync(Guid id, IEnumerable<String> eventKeys)
    {
        return Handle(id, async context =>
        {
            foreach (var eventKey in eventKeys)
                await context.HandleEventAsync(eventKey, null);
        });
    }

    static void SetInstanceState(IInstance inst, ExecutionContext context)
    {
        inst.Result = context.GetResult();
        inst.State = context.GetState();
        inst.ExecutionStatus = context.GetExecutionStatus();
        inst.CorrelationId = context.GetCorrelationId(inst.State);
        var instData = new InstanceData()
        {
            ExternalVariables = context.GetExternalVariables(inst.State),
            ExternalBookmarks = context.GetExternalBookmarks(),
            ExternalEvents = context.GetExternalEvents(),
            TrackRecords = context.GetTrackRecords(),
            Deferred = context.GetDeferred(),
            Inboxes = context.GetInboxes()
        };
        inst.InstanceData = instData;
    }

    public async ValueTask ProcessPending()
    {
        var pend = await _instanceStorage.GetPendingAsync();
        if (pend == null)
            return;
        foreach (var asw in pend.AutoStart)
        {
            var inst = await AutoStartAsync(asw);
            await _instanceStorage.AutoStartComplete(asw.Id, inst.Id);
        }
        foreach (var pi in pend.Pending)
        {
            _logger.LogInformation("Process pending at {Time}, InstanceId {instanceId}", DateTime.Now, pi.InstanceId);
            await HandleEventsAsync(pi.InstanceId, pi.EventKeys);
        }
    }

    private async ValueTask<IInstance> AutoStartAsync(IAutoStartInstance autoStart)
    {
        _logger.LogInformation("Auto start process at {Time}, WorkflowId {WorkflowId}", DateTime.Now, autoStart.WorkflowId);
        if (String.IsNullOrEmpty(autoStart.WorkflowId))
            throw new InvalidProgramException("WorkflowId is null");
        var inst = await CreateAsync(new WorkflowIdentity(id: autoStart.WorkflowId, ver: autoStart.Version), autoStart.CorrelationId, null, autoStart.InstanceId);
        return await RunAsync(inst, autoStart.Params);
    }

    private async ValueTask<IInstance> Handle(Guid id, Func<ExecutionContext, ValueTask> action)
    {
        try
        {
            var inst = await _instanceStorage.Load(id);
            return await Handle(inst, action);
        }
        catch (Exception ex)
        {
            await _instanceStorage.WriteException(id, ex);
            throw;
        }
    }

    public async ValueTask<IInstance> LoadInstanceRaw(Guid id)
    {
        var inst = await _instanceStorage.LoadRaw(id);
        inst.Workflow.Root.OnEndInit(null);
        var context = new ExecutionContext(_serviceProvider, _tracker, inst);
        context.SetState(inst.State);
        SetInstanceState(inst, context);
        return inst;
    }

    private async ValueTask<IInstance> Handle(IInstance inst, Func<ExecutionContext, ValueTask> action)
    {
        inst.Workflow.Root.OnEndInit(null);
        var context = new ExecutionContext(_serviceProvider, _tracker, inst);
        context.SetState(inst.State);
        await action(context);
        await context.RunAsync();
        SetInstanceState(inst, context);
        await _instanceStorage.Save(inst);
        await CheckParent(inst);
        return inst;
    }

    public ValueTask<IInstance> SendMessageAsync(Guid id, String message)
    {
        return Handle(id, context => context.HandleMessageAsync(message));
    }

    private async ValueTask CheckParent(IInstance inst)
    {
        if (inst.Parent == null || inst.ExecutionStatus != WorkflowExecutionStatus.Complete)
            return;
        String bookmarkName = $"{inst.Workflow.Identity.Id}:{inst.Id}";
        var foundInst = await _instanceStorage.LoadBookmark(bookmarkName);
        if (foundInst == null)
            return;
        try
        {
            await Handle(foundInst, context => context.ResumeAsync(bookmarkName, inst.Result));
        }
        catch (Exception ex)
        {
            await _instanceStorage.WriteException(foundInst.Id, ex);
        }
    }
}

