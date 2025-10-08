// Copyright © 2020-2022 Oleksandr Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.SqlServer.Tests;

[TestClass]
[TestCategory("Storage.PendingMessage")]
public class PendingMessage
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkflowStorage _workflowStorage;
    private readonly IWorkflowCatalog _workflowCatalog;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IDbContext _dbContext;

    public PendingMessage()
    {
        _serviceProvider = TestEngine.ServiceProvider();
        _workflowStorage = _serviceProvider.GetRequiredService<IWorkflowStorage>();
        _workflowCatalog = _serviceProvider.GetRequiredService<IWorkflowCatalog>();
        _workflowEngine = _serviceProvider.GetRequiredService<IWorkflowEngine>();
        _dbContext = _serviceProvider.GetRequiredService<IDbContext>();
    }


    [TestMethod]
    public async Task PendingMessageSuccess()
    {
        String TestId = "PendingMessage";
        await TestEngine.PrepareDatabase(TestId);
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\messages\\boundary.bpmn");

        var inst = await TestEngine.SimpleRun(TestId, xaml);
        Assert.IsNotNull(inst);
        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);
        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.HasCount(2, log);
        Assert.AreEqual("startProcess|startSub", String.Join('|', log));


        await _dbContext.ExecuteExpandoAsync(null, "a2wf.[PendingMessage.Create]",
            new ExpandoObject()
            {
                {"InstanceId", inst.Id },
                {"Message", "Message1" }
            });

        await _workflowEngine.ProcessPending();

        var instRaw = await _workflowEngine.LoadInstanceRaw(inst.Id);

        log = instRaw.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.HasCount(4, log);
        Assert.AreEqual("startProcess|startSub|messageBoundary|endBoundary", String.Join('|', log));
    }
}

