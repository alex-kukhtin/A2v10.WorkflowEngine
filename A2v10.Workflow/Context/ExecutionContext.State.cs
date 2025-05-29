// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

using System.Collections.Generic;
using System.Dynamic;


namespace A2v10.Workflow;
public partial class ExecutionContext : IExecutionContext
{
    ExpandoObject? GetActivityStates()
    {
        var actState = new ExpandoObject();
        foreach (var (refer, activity) in _activities)
        {
            if (activity is IStorable storable)
            {
                ActivityStorage storage = new(StorageState.Storing);
                if (activity is not ICanComplete canComplete || !canComplete.IsComplete)
                    storable.Store(storage);
                if (storage.Value.IsNotEmpty())
                    actState.Set(refer, storage.Value);
            }
        }
        if (actState.IsEmpty())
            return null;
        return actState;
    }

    void SetActivityStates(ExpandoObject? state)
    {
        if (state == null)
            return;
        foreach (var refer in state.Keys())
        {
            if (_activities.TryGetValue(refer, out IActivity? activity))
            {
                if (activity is IStorable storable)
                {
                    var storage = new ActivityStorage(StorageState.Loading, state.Get<ExpandoObject>(refer));
                    storable.Restore(storage);
                }
            }
        }
    }

    Object? GetLastResult()
    {
        return _script.GetLastResult();
    }

    ExpandoObject? GetScriptVariables()
    {
        var vars = new ExpandoObject();
        foreach (var (refer, activity) in _activities)
        {
            if (activity is IScoped)
                vars.SetNotNull(refer, _script.Evaluate<ExpandoObject>(refer, "Store"));
        }
        if (vars.IsEmpty())
            return null;
        return vars;
    }

    void SetScriptVariables(ExpandoObject? vars)
    {
        if (vars == null)
            return;
        foreach (var refer in vars.Keys())
        {
            if (_activities.TryGetValue(refer, out IActivity? activity))
                if (activity is IScoped)
                    _script.Restore(refer, vars.GetNotNull<ExpandoObject>(refer));
        }
    }

    ExpandoObject? GetEndEvent()
    {
        return _endEvent;
    }

    ExpandoObject? GetEvents()
    {
        if (_events == null || _events.Count == 0)
            return null;
        var res = new ExpandoObject();
        foreach (var (k, v) in _events)
        {
            var eo = new ExpandoObject();
            eo.Set("Action", CallbackItem.CreateFrom(v.Action));
            eo.Set("Event", v.Event.ToExpando());
            res.Set(k, eo);
        }
        return res;
    }

    ExpandoObject? GetBookmarks()
    {
        if (_bookmarks == null || _bookmarks.Count == 0)
            return null;
        var res = new ExpandoObject();
        foreach (var b in _bookmarks)
            res.Set(b.Key, CallbackItem.CreateFrom(b.Value.Action));
        return res;
    }

    void SetBookmarks(ExpandoObject? marks)
    {
        if (marks == null)
            return;
        foreach (var k in marks.Keys())
        {
            var ebm = marks.Get<ExpandoObject>(k) ?? throw new InvalidProgramException("Bookmark is null");
            var cb = CallbackItem.FromExpando(ebm);
            if (_activities.TryGetValue(cb.Ref, out IActivity? activity))
                _bookmarks.Add(k, new BookmarkItem(activity.Id, cb.ToBookmark(activity)));
            else
                throw new WorkflowException($"Activity {cb.Ref} for bookmark callback not found");
        }
    }

    void SetEvents(ExpandoObject? events)
    {
        if (events == null)
            return;
        foreach (var k in events.Keys())
        {
            var ebm = events.GetNotNull<ExpandoObject>(k);
            var cb = CallbackItem.FromExpando(ebm.GetNotNull<ExpandoObject>("Action"));
            var wfe = WorkflowEventImpl.FromExpando(k, ebm.GetNotNull<ExpandoObject>("Event"));
            if (_activities.TryGetValue(cb.Ref, out IActivity? activity))
                _events.Add(k, new EventItem(cb.ToEvent(activity), wfe));
            else
                throw new WorkflowException($"Activity {cb.Ref} for event callback not found");
        }
    }

    public ExpandoObject GetState()
    {
        var res = new ExpandoObject();
        res.SetNotNull("State", GetActivityStates());
        res.SetNotNull("Variables", GetScriptVariables());
        res.SetNotNull("Bookmarks", GetBookmarks());
        res.SetNotNull("Events", GetEvents());
        res.SetNotNull("EndEvent", GetEndEvent());
        res.SetNotNull("LastResult", GetLastResult());
        return res;
    }

    private List<IVariable>? GetVariables()
    {
        List<IVariable>? variables = null;
        if (_root is IExternalScoped extScoped)
            variables = extScoped.ExternalVariables();
        else if (_root is IScoped scoped)
            variables = scoped.Variables;
        if (variables == null || variables.Count == 0)
            return null;
        return variables;
    }

    public String? GetCorrelationId(ExpandoObject state)
    {
        List<IVariable>? variables = GetVariables();
        if (variables == null)
            return null;
        var corrId = variables.Where(v => v.CorrelationId).FirstOrDefault();
        if (corrId == null)
            return null;
        var values = state.Get<ExpandoObject>("Variables");
        if (values == null)
            return null;
        var rootValues = values.Get<ExpandoObject>(_root.Id);
        var val = rootValues.Get<Object>(corrId.Name);
        if (val == null)
            return null;
        return val.ToString();
    }

    public ExpandoObject? GetExternalVariables(ExpandoObject state)
    {
        List<IVariable>? variables = GetVariables();
        if (variables == null)
            return null;
        var result = new ExpandoObject();
        var values = state.Get<ExpandoObject>("Variables");
        if (values == null)
            return null;
        var rootValues = values.Get<ExpandoObject>(_root.Id);

        void AddList(VariableType varType, String propName)
        {
            var list = new List<Object>();
            foreach (var v in variables.Where(v => v.External && v.Type == varType))
            {
                var ve = new ExpandoObject()
                {
                    {"Name", v.Name}
                };
                if (v is IExternalVariable extVar)
                {
                    var rv = values.Get<ExpandoObject>(extVar.ActivityId);
                    ve.Set("Value", rv.Get<Object>(v.Name));
                }
                else
                    ve.Set("Value", rootValues.Get<Object>(v.Name));
                list.Add(ve);
            }
            if (list.Count > 0)
                result.Set(propName, list);

        };

        AddList(VariableType.BigInt, "BigInt");
        AddList(VariableType.String, "String");
        AddList(VariableType.Guid, "Guid");
        if (result.IsEmpty())
            return null;
        return result;
    }

    public List<Object>? GetExternalEvents()
    {
        if (_events == null || _events.Count == 0)
            return null;
        var list = new List<Object>();
        foreach (var (_, v) in _events)
            list.Add(v.Event.ToStore());
        return list;
    }

    public List<Object>? GetExternalBookmarks()
    {
        if (_bookmarks == null || _bookmarks.Count == 0)
            return null;
        var list = new List<Object>();
        foreach (var b in _bookmarks)
            list.Add(
                new ExpandoObject() {
                    { "Bookmark", b.Key },
                    { "Activity", b.Value.Activity }
                }
            );
        return list;
    }

    public WorkflowExecutionStatus GetExecutionStatus()
    {
        WorkflowExecutionStatus status = WorkflowExecutionStatus.Complete;
        if (_bookmarks.Count > 0 || _events.Count > 0)
            status = WorkflowExecutionStatus.Idle;
        return status;
    }

    public void SetState(ExpandoObject? state)
    {
        if (state == null)
            return;
        SetActivityStates(state.Get<ExpandoObject>("State"));
        SetScriptVariables(state.Get<ExpandoObject>("Variables"));
        SetBookmarks(state.Get<ExpandoObject>("Bookmarks"));
        SetEvents(state.Get<ExpandoObject>("Events"));
        _endEvent = state.Get<ExpandoObject>("EndEvent");
    }

	public List<DeferredElement>? GetDeferred()
    {
        if (_script == null)
            return null;
        return _script.GetDeferred();
    }

	public List<Object>? GetTrackRecords()
    {
        var records = _tracker.Records;
        if (records == null)
            return null;

        var lst = new List<Object>();
        for (var i = 0; i < records.Count; i++)
            lst.Add(records[i].ToExpandoObject(i + 1 /*1-based*/));

        if (lst.Count == 0)
            return null;
        return lst;
    }

    public DeferredInboxes? GetInboxes()
    {
        if (_inboxCreate.Count == 0 && _inboxRemove.Count == 0)
            return null;
        return new DeferredInboxes(_inboxCreate, _inboxRemove);
    }
}

