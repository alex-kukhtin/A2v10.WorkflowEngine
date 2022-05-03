// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;

namespace A2v10.Workflow.SqlServer.Tests;

[TestClass]
[TestCategory("Storage.AutoStart")]
public class AutoStart
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkflowStorage _workflowStorage;
    private readonly IWorkflowCatalog _workflowCatalog;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IDbContext _dbContext;

    public AutoStart()
    {
        _serviceProvider = TestEngine.ServiceProvider();
        _workflowStorage = _serviceProvider.GetRequiredService<IWorkflowStorage>();
        _workflowCatalog = _serviceProvider.GetRequiredService<IWorkflowCatalog>();
        _workflowEngine = _serviceProvider.GetRequiredService<IWorkflowEngine>();
        _dbContext = _serviceProvider.GetRequiredService<IDbContext>();
    }


    [TestMethod]
    public async Task SimpleAutoStart()
    {
        String TestId = "plus5";
        await TestEngine.PrepareDatabase(TestId);
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\simple.bpmn");
        var format = "text/xml";

        await _workflowCatalog.SaveAsync(new WorkflowDescriptor(TestId, xaml, format));
        var ident = await _workflowStorage.PublishAsync(_workflowCatalog, TestId);

        Assert.AreEqual(1, ident.Version);

        await _dbContext.ExecuteExpandoAsync(null, "a2wf.[AutoStart.Create]",
            new ExpandoObject()
            {
                {"WorkflowId", TestId }
            });

        await _workflowEngine.ProcessPending();

        async Task AssertModel(Int64 val)
        {
            var instModel = await _dbContext.LoadModelAsync(null, "a2wf_test.AutoStartLast");
            Assert.AreEqual("Complete", instModel.Eval<String>("Instance.ExecutionStatus"));
            String? state = instModel.Eval<String>("Instance.State");
            Assert.IsNotNull(state);
            var stateObj = JsonConvert.DeserializeObject<ExpandoObject>(state);
            Assert.IsNotNull(stateObj);
            var intVal = stateObj.Eval<Int64>("Variables.Process_1.X");
            Assert.AreEqual(val, intVal);
        }

        await AssertModel(10);

        // with params and concrete version
        await _dbContext.ExecuteExpandoAsync(null, "a2wf.[AutoStart.Create]",
            new ExpandoObject()
            {
                {"WorkflowId", TestId},
                {"Version", 1},
                {"Params", "{\"X\": 123}"},
            });

        await _workflowEngine.ProcessPending();
        await AssertModel(128);
    }
}

