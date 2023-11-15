// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;
using System.Reflection;

namespace A2v10.Workflow;

using EventAction = Func<IExecutionContext, IWorkflowEvent, Object?, ValueTask>;
using ResumeAction = Func<IExecutionContext, String, Object?, ValueTask>;

public class CallbackItem(String refer, String callbacName)
{
    public String Ref = refer;
    public String CallbackName = callbacName;

    public static CallbackItem FromExpando(ExpandoObject eobj)
    {
        var cb = new CallbackItem(
            refer: eobj.Get<String>(nameof(Ref)) ?? throw new InvalidProgramException("Ref is null"),
            callbacName: eobj.Get<String>(nameof(CallbackName)) ?? throw new InvalidProgramException("CallbackName is null")
        );
        return cb;
    }

    public static ExpandoObject CreateFrom(Delegate callback)
    {
        if (callback.Target is not IActivity activityTarget)
            throw new InvalidOperationException("callback.Target must be an IActivity");

        var refer = activityTarget.Id;

        var custAttr = (StoreNameAttribute?)callback.Method.GetCustomAttributes(inherit: true)
                ?.FirstOrDefault(attr => attr is StoreNameAttribute) 
            ?? throw new InvalidOperationException("callback.Method has no StoreName attribute");
        var cb = new CallbackItem
        (
            refer: refer,
            callbacName: custAttr.Name
        );
        return cb.ToExpando();
    }

    private ExpandoObject ToExpando()
    {
        var eo = new ExpandoObject();
        eo.Set(nameof(Ref), Ref);
        eo.Set(nameof(CallbackName), CallbackName);
        return eo;
    }

    private MethodInfo GetMethod(IActivity activity)
    {
        foreach (var mi in activity.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var custAttr = (StoreNameAttribute?)mi.GetCustomAttributes(inherit: true)
                ?.FirstOrDefault(attr => attr is StoreNameAttribute);
            if (custAttr?.Name == CallbackName)
                return mi;
        }
        throw new WorkflowException($"Method '{CallbackName}' for activity '{Ref}' not found");
    }

    public ResumeAction ToBookmark(IActivity activity)
    {
        var mi = GetMethod(activity);
        return Delegate.CreateDelegate(typeof(ResumeAction), activity, mi)
            as ResumeAction ?? throw new WorkflowException("Invalid Resume Action");
    }

    public EventAction ToEvent(IActivity activity)
    {
        var mi = GetMethod(activity);
        return Delegate.CreateDelegate(typeof(EventAction), activity, mi) as EventAction
            ?? throw new WorkflowException("Invalid Event Action");
    }
}

