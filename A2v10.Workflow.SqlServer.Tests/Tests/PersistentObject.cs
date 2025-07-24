// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;

namespace A2v10.Workflow.SqlServer.Tests;

[TestClass]
[TestCategory("Storage.PersistentObject")]
public class PersistentObjects
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkflowEngine _workflowEngine;

    public PersistentObjects()
    {
        _serviceProvider = TestEngine.ServiceProvider();
        _workflowEngine = _serviceProvider.GetRequiredService<IWorkflowEngine>();
    }


    [TestMethod]
    public async Task SimplePersistent()
    {
        String wfId = "Persistent";

        await TestEngine.PrepareDatabase(wfId);
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\persistentObject\\simplePersistentObject.bpmn");

        var args = new ExpandoObject()
        {
            { "Order", new ExpandoObject()
                {
                    { "Id", (Int64) 77 },
                    { "Name", "Test"  },
                    { "Date", new DateTime(2025, 1, 1) }
                }
            }
        };

        var inst = await TestEngine.SimpleRun(wfId, xaml, args);

        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);


        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(1, log!.Length);
        Assert.AreEqual("start", String.Join('|', log));

        var order = inst.Result?.GetNotNull<ExpandoObject>("Order");
        Assert.AreEqual((Int64)77, order.Get<Int64>("Id"));
        Assert.AreEqual("Test", order.Get<String>("Name"));

        var sp = TestEngine.ServiceProvider();
        var engine = sp.GetRequiredService<IWorkflowEngine>();
        inst = await engine.ResumeAsync(inst.Id, "Inbox");

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);

        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(5, log!.Length);
        order = inst.Result?.GetNotNull<ExpandoObject>("Order");
        Assert.AreEqual((Int64)77, order.Get<Int64>("Id"));
        Assert.AreEqual("Data from SQL", order.Get<String>("Name"));
    }
}

