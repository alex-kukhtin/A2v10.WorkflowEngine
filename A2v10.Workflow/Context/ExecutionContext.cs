// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.Workflow.Bpmn;
using A2v10.Workflow.Tracker;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;

namespace A2v10.Workflow;

using EventAction = Func<IExecutionContext, IWorkflowEvent, Object?, ValueTask>;
using ResumeAction = Func<IExecutionContext, String, Object?, ValueTask>;

public record QueueItem
(
    Func<IExecutionContext, IToken?, ValueTask> Action,
    IActivity Activity,
    IToken? Token
);

public record EventItem(EventAction Action, IWorkflowEvent Event);

public partial class ExecutionContext : IExecutionContext
{
    private readonly Queue<QueueItem> _commandQueue = new();
    private readonly Dictionary<String, IActivity> _activities = new();
    private readonly Dictionary<String, ResumeAction> _bookmarks = new();
    private readonly Dictionary<String, EventItem> _events = new();
    private readonly List<ExpandoObject> _inboxCreate = new();
    private readonly List<Guid> _inboxRemove = new();

    private readonly IActivity _root;
    private readonly IInstance _instance;

    private readonly ScriptEngine _script;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITracker _tracker;
    private readonly IWorkflowEngine _engine;
    private readonly IInstanceStorage _instanceStorage;

    private ExpandoObject? _endEvent;

    public ExecutionContext(IServiceProvider serviceProvider, ITracker tracker, IInstance instance, Object? args = null)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _tracker = tracker;
        _instance = instance;
        _root = instance.Workflow.Root;
        _engine = _serviceProvider.GetRequiredService<IWorkflowEngine>();
        _instanceStorage = _serviceProvider.GetRequiredService<IInstanceStorage>();

        // store all activites
        var toMapArg = new TraverseArg()
        {
            Action = (activity) => _activities.Add(activity.Id, activity)
        };
        _root.Traverse(toMapArg);

        SetCorrelationId(); // before script

        _script = BuildScript(args);
        _tracker.Start();
    }
    void SetCorrelationId()
    {
        if (_instance.CorrelationId == null)
            return;
        if (_root is not IScoped rootScoped)
            return;
        var corrVariable = rootScoped.Variables?.FirstOrDefault(v => v.CorrelationId);
        if (corrVariable != null)
            corrVariable.Value = _instance.CorrelationId;
    }

    ScriptEngine BuildScript(Object? args)
    {
        if (_root is not IScoped)
            throw new InvalidProgramException("Root is not IScoped");
        var sb = new ScriptBuilder();
        var sbTraverseArg = new TraverseArg()
        {
            Start = (activity) => sb.Start(activity),
            Action = (activity) => sb.Build(activity),
            End = (activity) => sb.End(activity)
        };
        _root.Traverse(sbTraverseArg);
        sb.EndScript();
        return new ScriptEngine(_serviceProvider, _tracker, _root, sb.Script, args);
    }

    public ExpandoObject? GetResult()
    {
        var res = _script.GetResult();
        if (_endEvent != null)
        {
            if (res == null)
                res = new ExpandoObject();
            res.Set("EndEvent", _endEvent);
        }
        return res;
    }

    #region IExecutionContext
    public void Schedule(IActivity? activity, IToken? token)
    {
        if (activity == null)
            return;
        _tracker.Track(new ActivityTrackRecord(ActivityTrackAction.Schedule, activity, token));
        _commandQueue.Enqueue(new QueueItem(activity.ExecuteAsync, activity, token));
    }

    public void SetBookmark(String bookmark, IActivity activity, ResumeAction onComplete)
    {
        _tracker.Track(new ActivityTrackRecord(ActivityTrackAction.Bookmark, activity, $"{{bookmark:'{bookmark}'}}"));
        _bookmarks.Add(bookmark, onComplete);
    }

    public void RemoveBookmark(String bookmark)
    {
        if (_bookmarks.ContainsKey(bookmark))
            _bookmarks.Remove(bookmark);
    }

    public void SetInbox(Guid id, ExpandoObject inbox, IActivity activity)
    {
        _tracker.Track(new ActivityTrackRecord(ActivityTrackAction.Inbox, activity, $"{{inbox:'{id}'}}"));
        var eo = inbox.Clone();
        eo.SetOrReplace("Id", id);
        eo.SetOrReplace("Bookmark", activity.Id);
        _inboxCreate.Add(eo);
    }

    public void RemoveInbox(Guid? id)
    {
        if (id == null)
            return;
        _inboxRemove.Add(id.Value);
    }

    public void AddEvent(IWorkflowEvent wfEvent, IActivity activity, EventAction onTrigger)
    {
        _tracker.Track(new ActivityTrackRecord(ActivityTrackAction.Event, activity, wfEvent.ToString()));
        _events.Add(wfEvent.Key, new EventItem(onTrigger, wfEvent));
    }

    public void RemoveEvent(String eventKey)
    {
        if (_events.ContainsKey(eventKey))
            _events.Remove(eventKey);
    }

    public T? Evaluate<T>(String refer, String name)
    {
        return _script.Evaluate<T>(refer, name);
    }

    public void Execute(String refer, String name)
    {
        _script.Execute(refer, name);
    }

    public void ExecuteResult(String refer, String name, Object? result)
    {
        _script.ExecuteResult(refer, name, result);
    }

    public void SetVariable(String refer, String name, Object? value)
    {
        _script.SetVariable(refer, name, value);
    }
    #endregion

    public async ValueTask RunAsync()
    {
        while (_commandQueue.Count > 0)
        {
            var queueItem = _commandQueue.Dequeue();
            _tracker.Track(new ActivityTrackRecord(ActivityTrackAction.Execute, queueItem.Activity, queueItem.Token));
            await queueItem.Action(this, queueItem.Token);
        }
        _tracker.Stop();
    }

    public ValueTask ResumeAsync(String bookmark, Object? result)
    {
        if (_bookmarks.TryGetValue(bookmark, out ResumeAction? action))
        {
            String strResult = result != null ? $", result:{JsonSerializer.Serialize(result)}" : String.Empty;
            _tracker.Track(new ActivityTrackRecord(ActivityTrackAction.Resume, null, $"{{bookmark:'{bookmark}'{strResult}}}"));
            return action(this, bookmark, result);
        }
        else
            throw new WorkflowException($"Bookmark '{bookmark}' not found");
    }

    public void ProcessEndEvent(IWorkflowEvent evt)
    {
        switch (evt.Kind)
        {
            case EventKind.Error:
                var err = _instance?.Workflow?.Wrapper?.FindElement<Error>(e => e.Id == evt.Ref);
                if (err != null)
                {
                    _endEvent = new ExpandoObject()
                    {
                        { "Kind", "Error" },
                        { "Error", err.Name },
                        { "ErrorCode", err.ErrorCode }
                    };
                }
                break;
            case EventKind.Escalation:
                var esc = _instance.Workflow?.Wrapper?.FindElement<Escalation>(e => e.Id == evt.Ref);
                if (esc != null)
                {
                    _endEvent = new ExpandoObject()
                    {
                        { "Kind", "Escalation" },
                        { "Escalation", esc.Name },
                        { "EscalationCode", esc.EscalationCode }
                    };
                }
                break;
            case EventKind.Message:
                var msg = _instance?.Workflow?.Wrapper?.FindElement<Message>(m => m.Id == evt.Ref);
                //throw new NotImplementedException("EndEvent (Message)");
                break;
        }
    }

    public async ValueTask HandleMessageAsync(String message)
    {
        // MessageName => MessageId
        var msg = _instance?.Workflow?.Wrapper?.FindElement<Message>(m => m.Name == message);
        if (msg == null)
            return;
        foreach (var (eventKey, eventItem) in _events)
        {
            if (eventItem.Event.Ref == msg.Id)
            {
                _tracker.Track(new ActivityTrackRecord(ActivityTrackAction.HandleMessage, null, $"{{message:'{message}', event:{eventKey}}}"));
                await eventItem.Action(this, eventItem.Event, null);
            }
        }
    }

    public ValueTask HandleEventAsync(String eventKey, Object? result)
    {
        if (_events.TryGetValue(eventKey, out EventItem? eventItem))
        {
            String strResult = result != null ? $", result:{JsonSerializer.Serialize(result)}" : String.Empty;
            _tracker.Track(new ActivityTrackRecord(ActivityTrackAction.HandleEvent, null, $"{{event:'{eventKey}'{strResult}}}"));
            return eventItem.Action(this, eventItem.Event, result);
        }
        return ValueTask.CompletedTask; // Possibly already done!
    }

    public async ValueTask HandleEvent(IWorkflowEvent evt)
    {
        foreach (var (eventKey, eventItem) in _events)
        {
            if (eventItem.Event.Ref == evt.Ref)
            {
                _tracker.Track(new ActivityTrackRecord(ActivityTrackAction.HandleMessage, null, $"{{message:'{evt.Ref}', event:{eventKey}}}"));
                await eventItem.Action(this, eventItem.Event, null);
            }
        }
    }

    public async ValueTask HandleEndEvent(ExpandoObject? evt)
    {
        if (evt == null)
            return;
        var kind = evt.Get<String>("Kind");
        switch (kind)
        {
            case "Error":
                var errname = evt.GetNotNull<String>("Error");
                var err = _instance?.Workflow?.Wrapper?.FindElement<Error>(e => e.Name == errname);
                if (err != null)
                    await HandleEvent(new WorkflowErrorEvent(errname, err.Id));
                break;
            case "Escalation":
                var escname = evt.GetNotNull<String>("Escalation");
                var esc = _instance?.Workflow?.Wrapper?.FindElement<Escalation>(e => e.Name == escname);
                if (esc != null)
                    await HandleEvent(new WorkflowEscalationEvent(escname, esc.Id));
                break;

        }
    }

    public async ValueTask<IInstance> Call(String activity, ExpandoObject? prms)
    {
        var ea = ExternalActivity.Parse(activity);
        if (ea.IsBpmn)
        {
            if (ea.WorkflowIdentity == null)
                throw new InvalidProgramException("WorkflowIdentity is null");
            var correlationId = prms.Get<String>("CorrelationId");
            var inst = await _engine.CreateAsync(ea.WorkflowIdentity, correlationId, _instance.Id);
            var result = await _engine.RunAsync(inst.Id, prms);
            return result;
        }
        else
            throw new WorkflowException($"ExecutionContext.Call Invalid activity '{activity}'");
    }

    public ValueTask<DateTime> Now()
	{
        return _instanceStorage.GetNowTime();
	}
}

