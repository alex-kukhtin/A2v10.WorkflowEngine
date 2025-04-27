// Copyright © 2021-2025 Oleksandr Kukhtin. All rights reserved.

using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

using A2v10.Runtime.Interfaces;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Engine;

public class WorkflowInvokeTarget(IWorkflowEngine _engine, IWorkflowStorage _storage, IWorkflowCatalog _catalog) : IRuntimeInvokeTarget
{
    public static class Properties
    {
        public const String WorkflowId = nameof(WorkflowId);
        public const String InstanceId = nameof(InstanceId);
        public const String Args = nameof(Args);
        public const String Bookmark = nameof(Bookmark);
        public const String Reply = nameof(Reply);
        public const String Result = nameof(Result);
        public const String Body = nameof(Body);
        public const String Format = nameof(Format);
        public const String Version = nameof(Version);
    }

    public async Task<ExpandoObject> CreateAsync(String workflowId, Int32 version = 0)
    {
        if (String.IsNullOrEmpty(workflowId))
            throw new WorkflowException($"Start. WorkflowId is required");
        var res = await _engine.CreateAsync(new WorkflowIdentity(workflowId, version));

        return new ExpandoObject()
        {
            { Properties.InstanceId, res.Id }
        };
    }

    public async Task<ExpandoObject> RunAsync(Object? instanceId, ExpandoObject? args)
    {
        if (instanceId == null)
            throw new WorkflowException($"Run. InstanceId is required");
        Guid instanceGuid = instanceId switch
        {
            Guid guidVal => guidVal,
            String strVal => Guid.Parse(strVal),
            _ => throw new WorkflowException($"Run.InstanceId invalid type")
        };
        if (instanceGuid == Guid.Empty)
            throw new WorkflowException($"Run. InstanceId is required");
        var res = await _engine.RunAsync(instanceGuid, args);
        return new ExpandoObject()
        {
            { Properties.InstanceId, res.Id },
            { Properties.Result, res.Result}
        };
    }

    public async Task<ExpandoObject> ResumeAsync(Object? instanceId, String bookmark, Object? reply)
    {
        if (instanceId == null)
            throw new WorkflowException($"Resume. InstanceId is required"); 
        Guid instanceGuid = instanceId switch
        {
            Guid guidVal => guidVal,
            String strVal => Guid.Parse(strVal),
            _ => throw new WorkflowException($"Resume.InstanceId invalid type")
        };
        if (instanceGuid == Guid.Empty)
            throw new WorkflowException($"Resume. InstanceId is required");
        var res = await _engine.ResumeAsync(instanceGuid, bookmark, reply);
        return new ExpandoObject()
        {
            { Properties.InstanceId, res.Id },
            { Properties.Result, res.Result}
        };
    }

    public async Task<ExpandoObject> Variables(Object? instanceId)
    {
        if (instanceId == null)
            throw new WorkflowException($"Variables. InstanceId is required");
        Guid instanceGuid = instanceId switch
        {
            Guid guidVal => guidVal,
            String strVal => Guid.Parse(strVal),
            _ => throw new WorkflowException($"Variables.InstanceId invalid type")
        };
        if (instanceGuid == Guid.Empty)
            throw new WorkflowException($"Variables. InstanceId is required");
        var instance = await _engine.LoadInstanceRaw(instanceGuid);
        return instance.State?.Get<ExpandoObject>("Variables") ?? new ExpandoObject();
    }


    public async Task<ExpandoObject> StartAsync(String workflowId, Int32 version, ExpandoObject? args)
    {
        if (String.IsNullOrEmpty(workflowId))
            throw new WorkflowException($"Start. WorkflowId is required");
        var resCreate = await CreateAsync(workflowId, version);
        var instId = resCreate.Get<Guid>(Properties.InstanceId);
        return await RunAsync(instId, args);
    }

    public async Task<ExpandoObject> SaveAsync(String workflowId, String format, String body)
    {
        await _catalog.SaveAsync(new WorkflowDescriptor(Id: workflowId, Body: body, Format: format));
        return [];
    }

    public async Task<ExpandoObject> PublishAsync(String workflowId)
    {
        var res = await _storage.PublishAsync(_catalog, workflowId);
        return new ExpandoObject()
        {
            { Properties.WorkflowId, res.Id},
            { Properties.Version, res.Version }
        };
    }

    public async Task<ExpandoObject> CheckSyntaxAsync(String workflowId)
    {
        var wfBody = await _catalog.LoadBodyAsync(workflowId);
        if (wfBody.Format != "xaml" && wfBody.Format != "text/xml")
            throw new InvalidOperationException("Invalid format");

        var root = _storage.LoadFromBody(wfBody.Body, wfBody.Format)
            ?? throw new InvalidOperationException("Activity is null");

        var errors = SyntaxChecker.CheckSyntax(root);

        var list = new List<ExpandoObject>();
        foreach (var err in errors)
        {
            list.Add(new ExpandoObject()
            {
                { "Message", err.Message },
                { "Activity", err.ActivityId },
                { "Script", err.Script }
            });
        }
        return new ExpandoObject() {
            { "Errors", list }
        };
    }

    public async Task<ExpandoObject> InvokeAsync(String method, ExpandoObject? parameters)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));
        return method switch
        {
            "Create" => await CreateAsync(
                    parameters.GetNotNull<String>(Properties.WorkflowId)
                ),
            "Run" => await RunAsync(
                    parameters.Get<Object>(Properties.InstanceId),
                    parameters.Get<ExpandoObject>(Properties.Args)
                ),
            "Resume" => await ResumeAsync(
                    parameters.Get<Object>(Properties.InstanceId),
                    parameters.GetNotNull<String>(Properties.Bookmark),
                    parameters.Get<ExpandoObject>(Properties.Reply)
                ),
            "Start" => await StartAsync(
                    parameters.GetNotNull<String>(Properties.WorkflowId),
                    parameters.Get<Int32>(Properties.Version),
                    parameters.Get<ExpandoObject>(Properties.Args)
                ),
            "Save" => await SaveAsync(
                    parameters.GetNotNull<String>(Properties.WorkflowId),
                    parameters.GetNotNull<String>(Properties.Format),
                    parameters.GetNotNull<String>(Properties.Body)
                ),
            "Publish" => await PublishAsync(
                    parameters.GetNotNull<String>(Properties.WorkflowId)
                ),
            "CheckSyntax" => await CheckSyntaxAsync(
                    parameters.GetNotNull<String>(Properties.WorkflowId)
                ),
            "Variables" => await Variables(
                    parameters.GetNotNull<Object>(Properties.InstanceId)
                ),
            _ => throw new WorkflowException($"Invalid target method '{method}'. Expected: Save, Publish, Create, Run, Start, Resume, CheckSyntax")
        };
    }
}

