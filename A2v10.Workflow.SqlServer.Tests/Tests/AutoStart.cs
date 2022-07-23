// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;

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
    public async Task AutoStartWithDate()
    {
        String TestId = "AutoStartDate";
        await TestEngine.PrepareDatabase(TestId);
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\autostartdate.bpmn");
        var format = "text/xml";
        await _workflowCatalog.SaveAsync(new WorkflowDescriptor(TestId, xaml, format));
        var ident = await _workflowStorage.PublishAsync(_workflowCatalog, TestId);

        Assert.AreEqual(1, ident.Version);

        await _dbContext.ExecuteExpandoAsync(null, "a2wf.[AutoStart.Create]",
            new ExpandoObject()
            {
                {"WorkflowId", TestId },
                {"Params", "{\"InDate\": \"2022-06-01T00:00:00Z\"}" }
            });

        await _workflowEngine.ProcessPending();
        var instModel = await _dbContext.LoadModelAsync(null, "a2wf_test.AutoStartLast",
            new ExpandoObject() {
                    {"WorkflowId", TestId }
         });
        var instId = instModel.Eval<Guid>("Instance.Id");

        var instRaw = await _workflowEngine.LoadInstanceRaw(instId);
        Assert.AreEqual("Wed Jun 01 2022 03:00:00 GMT+0300 (FLE Standard Time)", instRaw?.Result?.Eval<String>("OutDate"));
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
            var instModel = await _dbContext.LoadModelAsync(null, "a2wf_test.AutoStartLast",
                new ExpandoObject() {
                    {"WorkflowId", TestId }
             });
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

    [TestMethod]
    public async Task AutoStartAt()
	{
        var wfId = "AutoStartAt";
        await TestEngine.PrepareDatabase("AutoStartAt");
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\simple.bpmn");
        var ident = await TestEngine.SimplePublish(wfId, xaml);
        Assert.AreEqual(1, ident.Version);
        await _dbContext.ExecuteExpandoAsync(null, "a2wf.[AutoStart.Create]",
            new ExpandoObject() {
                {"WorkflowId", wfId },
                {"StartAt", DateTime.UtcNow + TimeSpan.FromSeconds(1) }
            });

        var prms = new ExpandoObject() {
            { "WorkflowId", wfId }
        };
        await _workflowEngine.ProcessPending();
        var instModel = await _dbContext.LoadModelAsync(null, "a2wf_test.AutoStartLast", prms);
        Assert.IsNull(instModel.Root.Get<Object>("Instance"));

        await Task.Delay(1100);
        await _workflowEngine.ProcessPending();
        instModel = await _dbContext.LoadModelAsync(null, "a2wf_test.AutoStartLast", prms);
        Assert.IsNotNull(instModel.Root.Get<Object>("Instance"));
        var instanceId = instModel.Root.Eval<Guid>("Instance.Id");
        var wfInst = await _workflowEngine.LoadInstanceRaw(instanceId);
        Assert.AreEqual(wfInst.ExecutionStatus, WorkflowExecutionStatus.Complete);
        Assert.AreEqual(10F, wfInst.Result?.Eval<Double>("X"));
    }


    [TestMethod]
    public async Task AutoStartAtTime()
    {
        var wfId = "AutoStartAt";
        await TestEngine.PrepareDatabase("AutoStartAt");
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\simple.bpmn");
        var ident = await TestEngine.SimplePublish(wfId, xaml);
        Assert.AreEqual(1, ident.Version);

        var now = DateTime.UtcNow;
        var dt = new DateTime(2022, 03, 16, now.Hour, now.Minute, now.Second, now.Millisecond);

        await _dbContext.ExecuteExpandoAsync(null, "a2wf.[CurrentDate.Set]",
            new ExpandoObject()
            {
                { "Date", dt }
            });

        await _dbContext.ExecuteExpandoAsync(null, "a2wf.[AutoStart.Create]",
            new ExpandoObject() {
                {"WorkflowId", wfId },
                {"StartAt", dt + TimeSpan.FromSeconds(1) }
            });

        var prms = new ExpandoObject() {
            { "WorkflowId", wfId }
        };
        await _workflowEngine.ProcessPending();
        var instModel = await _dbContext.LoadModelAsync(null, "a2wf_test.AutoStartLast", prms);
        Assert.IsNull(instModel.Root.Get<Object>("Instance"));

        await Task.Delay(1100);
        await _workflowEngine.ProcessPending();
        instModel = await _dbContext.LoadModelAsync(null, "a2wf_test.AutoStartLast", prms);
        Assert.IsNotNull(instModel.Root.Get<Object>("Instance"));
        var instanceId = instModel.Root.Eval<Guid>("Instance.Id");
        var wfInst = await _workflowEngine.LoadInstanceRaw(instanceId);
        Assert.AreEqual(wfInst.ExecutionStatus, WorkflowExecutionStatus.Complete);
        Assert.AreEqual(10F, wfInst.Result?.Eval<Double>("X"));
    }
}

