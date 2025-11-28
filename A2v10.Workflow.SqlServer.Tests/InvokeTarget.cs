// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Workflow.Interfaces;
using A2v10.Runtime.Interfaces;

namespace A2v10.Workflow.SqlServer.Tests;

[TestClass]
[TestCategory("Storage.InvokeTarget")]
public class InvokeTarget
{
    private readonly IServiceProvider _serviceProvider;

    public InvokeTarget()
    {
        _serviceProvider = TestEngine.ServiceProvider();

    }

    [TestInitialize]
    public void Init()
    {
    }

    [TestMethod]
    public async Task StartWorkflow_Error()
    {
        var id = "DummyWorkflow";
        var target = _serviceProvider.GetRequiredService<IRuntimeInvokeTarget>();
        var ex = await Assert.ThrowsExactlyAsync<SqlServerStorageException>(() =>
        {
            return target.InvokeAsync("Start", new ExpandoObject()
            {
                { "WorkflowId", id }
            });
        });
        Assert.AreEqual($"Workflow not found. (Id:'{id}', Version:0)", ex.Message);
    }

    [TestMethod]
    public async Task RunWorkflow_Error()
    {
        var target = _serviceProvider.GetRequiredService<IRuntimeInvokeTarget>();
        var ex = await Assert.ThrowsExactlyAsync<WorkflowException>(() =>
        {
            return target.InvokeAsync("Run", new ExpandoObject()
            {
                { "x", "5" }
            });
        });
        Assert.AreEqual("Run. InstanceId is required", ex.Message);
    }

    [TestMethod]
    public async Task CreateWorkflow_Error()
    {
        var id = "DummyWorkflow";
        var target = _serviceProvider.GetRequiredService<IRuntimeInvokeTarget>();
        var ex = await Assert.ThrowsExactlyAsync<SqlServerStorageException>(() =>
        {
            return target.InvokeAsync("Create", new ExpandoObject()
            {
                { "WorkflowId", id }
            });
        });
        Assert.AreEqual($"Workflow not found. (Id:'{id}', Version:0)", ex.Message);
    }

    [TestMethod]
    public async Task StartWorkflow_Success()
    {
        var id = "SimpleTarget";
        await TestEngine.PrepareDatabase(id);

        var target = _serviceProvider.GetRequiredService<IRuntimeInvokeTarget>();

        var format = "xaml";
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\simple.bpmn");

        await target.InvokeAsync("Save", new ExpandoObject()
        {
            { "WorkflowId", id },
            { "Format", format },
            { "Body", xaml }
        });

        await target.InvokeAsync("Publish", new ExpandoObject()
        {
            {"WorkflowId", id }
        });

        var res = await target.InvokeAsync("Start", new ExpandoObject()
        {
            {"WorkflowId", id },
            {"Args", new ExpandoObject()
                {
                    {"X", 5 }
                }
            }
        });

        Assert.AreEqual(10.0, res.Eval<Double>("Result.X"));
    }

    [TestMethod]
    public async Task ResumeWorkflow_Success()
    {
        var id = "SimpleTarget";
        await TestEngine.PrepareDatabase(id);

        var target = _serviceProvider.GetRequiredService<IRuntimeInvokeTarget>();

        var format = "xaml";
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\inbox\\inbox_1.bpmn");

        await target.InvokeAsync("Save", new ExpandoObject()
        {
            { "WorkflowId", id },
            { "Format", format },
            { "Body", xaml }
        });

        await target.InvokeAsync("Publish", new ExpandoObject()
        {
            {"WorkflowId", id }
        });

        var res = await target.InvokeAsync("Start", new ExpandoObject()
        {
            {"WorkflowId", id }
        }) ?? throw new InvalidOperationException("result is null");

        String? instanceId = res.Get<Object>("InstanceId")?.ToString();
        List<ExpandoObject>? signal = res.Get<List<ExpandoObject>>("Signal");

        Assert.IsNotNull(signal);
        Assert.HasCount(2, signal);

        var resResume = await target.InvokeAsync("Resume", new ExpandoObject()
        {
            {"InstanceId", instanceId },
            {"Bookmark", "Inbox" },
            {"Reply", new ExpandoObject()
                {
                    {"Value", 10.0 }
                }
            }       
        })  ?? throw new InvalidOperationException("resume is null");

        String? resInstanceId = resResume.Get<Object>("InstanceId")?.ToString();

        Assert.AreEqual(instanceId, resInstanceId);
    }


    [TestMethod]
    public async Task CheckSyntax_Error()
    {
        var id = "ScriptError";
        await TestEngine.PrepareDatabase(id);

        var target = _serviceProvider.GetRequiredService<IRuntimeInvokeTarget>();

        var format = "xaml";
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\ScriptError.bpmn");

        await target.InvokeAsync("Save", new ExpandoObject()
        {
            { "WorkflowId", id },
            { "Format", format },
            { "Body", xaml }
        });

        var res = await target.InvokeAsync("CheckSyntax", new ExpandoObject()
        {
            {"WorkflowId", id }
        }) ?? throw new InvalidOperationException("result is null");

        var errors = res.Get<List<ExpandoObject>>("Errors")
            ?? throw new InvalidOperationException("errors is null");    

        Assert.HasCount(2, errors);
    }

    [TestMethod]
    public async Task Target_GetVariables()
    {
        var id = "SimpleTarget";
        await TestEngine.PrepareDatabase(id);

        var target = _serviceProvider.GetRequiredService<IRuntimeInvokeTarget>();

        var format = "xaml";
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\simple.bpmn");

        await target.InvokeAsync("Save", new ExpandoObject()
        {
            { "WorkflowId", id },
            { "Format", format },
            { "Body", xaml }
        });

        await target.InvokeAsync("Publish", new ExpandoObject()
        {
            {"WorkflowId", id }
        });

        var res = await target.InvokeAsync("Start", new ExpandoObject()
        {
            {"WorkflowId", id },
            {"Args", new ExpandoObject()
                {
                    {"X", 5 }
                }
            }
        });

        var variables = await target.InvokeAsync("Variables", new ExpandoObject()
        {
            {"InstanceId", res.Get<Object>("InstanceId") }
        });

        Assert.AreEqual(10.0, res.Eval<Double>("Result.X"));
        Assert.AreEqual(10.0, variables.Eval<Double>("Process_1.X"));
    }

    [TestMethod]
    public async Task PersistentCorrelationId()
    {
        String wfId = "PersistentCorrelation";
        var format = "xaml";

        await TestEngine.PrepareDatabase(wfId);
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\persistentObject\\simplePersistentObjectCorrelation.bpmn");
        var target = _serviceProvider.GetRequiredService<IRuntimeInvokeTarget>();

        await target.InvokeAsync("Save", new ExpandoObject()
        {
            { "WorkflowId", wfId },
            { "Format", format },
            { "Body", xaml }
        });

        await target.InvokeAsync("Publish", new ExpandoObject()
        {
            {"WorkflowId", wfId }
        });

        var res = await target.InvokeAsync("Start", new ExpandoObject()
        {
            {"WorkflowId", wfId },
            {"CorrelationId", 99 },
            {"UserId", 55 }
        });

        var resResume = await target.InvokeAsync("Resume", new ExpandoObject()
        {
            {"UserId", 199 },
            {"InstanceId", res.Get<Object>("InstanceId") },
            {"Bookmark", "Inbox" },
            {"Reply", new ExpandoObject()
                {
                    { "Value",  10.0 },
                    { "UserId", "$(UserId)"}
                }
            }
        }) ?? throw new InvalidOperationException("resume is null");


        var variables = await target.InvokeAsync("Variables", new ExpandoObject()
        {
            {"InstanceId", resResume.Get<Object>("InstanceId") }
        });

        Assert.AreEqual(99L, variables.Eval<Int64>("Persist.Order.Id"));

    }


    [TestMethod]
    public async Task MessageWorkflow_Success()
    {
        var id = "SimpleTarget";
        await TestEngine.PrepareDatabase(id);

        var target = _serviceProvider.GetRequiredService<IRuntimeInvokeTarget>();

        var format = "xaml";
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\messages\\boundary.bpmn");

        await target.InvokeAsync("Save", new ExpandoObject()
        {
            { "WorkflowId", id },
            { "Format", format },
            { "Body", xaml }
        });

        await target.InvokeAsync("Publish", new ExpandoObject()
        {
            {"WorkflowId", id }
        });

        var res = await target.InvokeAsync("Start", new ExpandoObject()
        {
            {"WorkflowId", id }
        }) ?? throw new InvalidOperationException("result is null");

        String? instanceId = res.Get<Object>("InstanceId")?.ToString();

        var resMessage = await target.InvokeAsync("Message", new ExpandoObject()
        {
            {"InstanceId", instanceId },
            {"Message", "Message1" },
        }) ?? throw new InvalidOperationException("message is null");

        String? resInstanceId = resMessage.Get<Object>("InstanceId")?.ToString();

        Assert.AreEqual(instanceId, resInstanceId);

        var variables = await target.InvokeAsync("Variables", new ExpandoObject()
        {
            {"InstanceId", resInstanceId }
        });

        var log = variables.Eval<Object[]>("Process_1.log")?.Select(x => x.ToString()).ToArray();
        Assert.IsNotNull(log);
        Assert.HasCount(4, log);
        Assert.AreEqual("endBoundary", log[3]);
    }
}
