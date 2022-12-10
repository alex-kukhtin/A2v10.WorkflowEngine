// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace A2v10.Workflow.SqlServer.Tests;

[TestClass]
[TestCategory("Storage.ExternalVariables")]
public class ExternalVariables
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IDbContext _dbContext;

    public ExternalVariables()
    {
        _serviceProvider = TestEngine.ServiceProvider();
        _workflowEngine = _serviceProvider.GetRequiredService<IWorkflowEngine>();
        _dbContext = _serviceProvider.GetRequiredService<IDbContext>();

    }


    [TestMethod]
    public async Task SimpleExternal()
    {
        String wfId = "simpleExternal";

        await TestEngine.PrepareDatabase(wfId);
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\externalVariables\\external_simple.bpmn");
        var inst = await TestEngine.SimpleRun(wfId, xaml);

        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);

        var res = await _workflowEngine.LoadInstanceRaw(inst.Id);
        var dm = await _dbContext.LoadModelAsync(null, "a2wf_test.[Instance.External.Load]", new { InstanceId = inst.Id });

        Assert.AreEqual("String", res.State?.Eval<String>("Variables.Process_1.String"));
        Assert.AreEqual("String", dm.Eval<String>("Instance.Strings[0].Value"));

        Assert.AreEqual(5123, res.State?.Eval<Double>("Variables.Process_1.Ident"));
        Assert.AreEqual(5123, dm.Eval<Int64>("Instance.Integers[0].Value"));

        Assert.AreEqual("8783169c-2674-441f-963f-9be9c37eaa3f", res.State?.Eval<String>("Variables.Process_1.Guid"));
        Assert.AreEqual(new Guid("8783169C-2674-441F-963F-9BE9C37EAA3F"), dm.Eval<Guid>("Instance.Guids[0].Value"));

        inst = await _workflowEngine.ResumeAsync(inst.Id, "Bookmark1");

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        res = await _workflowEngine.LoadInstanceRaw(inst.Id);

        dm = await _dbContext.LoadModelAsync(null, "a2wf_test.[Instance.External.Load]", new { InstanceId = inst.Id });

        Assert.AreEqual("After", dm.Eval<String>("Instance.Strings[0].Value"));
        Assert.AreEqual("After", res.State?.Eval<String>("Variables.Process_1.String"));
        Assert.AreEqual(5123 + 5785, res.State?.Eval<Double>("Variables.Process_1.Ident"));
        Assert.AreEqual(5123 + 5785, dm.Eval<Int64>("Instance.Integers[0].Value"));

        Assert.AreEqual("163944CF-B074-46D3-882D-B41BB2A6FA73", res.State?.Eval<String>("Variables.Process_1.Guid"));
        Assert.AreEqual(new Guid("163944CF-B074-46D3-882D-B41BB2A6FA73"), dm.Eval<Guid>("Instance.Guids[0].Value"));

        //"163944CF-B074-46D3-882D-B41BB2A6FA73"
    }

    [TestMethod]
    public async Task CorrelationId()
    {
        String wfId = "correlationId";

        await TestEngine.PrepareDatabase(wfId);
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\externalVariables\\correlationId.bpmn");
        var inst = await TestEngine.SimpleRun(wfId, xaml);

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);

        var res = await _workflowEngine.LoadInstanceRaw(inst.Id);
        var dm = await _dbContext.LoadModelAsync(null, "a2wf_test.[Instance.External.Load]", new { InstanceId = inst.Id });

        Assert.AreEqual("9FBBB786-CFA2-4D74-9D39-EFFD6B2F41A0", res.CorrelationId);
        Assert.AreEqual("9FBBB786-CFA2-4D74-9D39-EFFD6B2F41A0", dm.Eval<String>("Instance.CorrelationId"));
        Assert.AreEqual("9FBBB786-CFA2-4D74-9D39-EFFD6B2F41A0", res.State?.Eval<String>("Variables.Process_1.ModelId"));

        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(3, log!.Length);
        Assert.AreEqual("start|task|end", String.Join('|', log));
    }

    [TestMethod]
    public async Task CreateCorrelationId()
    {
        String wfId = "createCorrelationId";

        await TestEngine.PrepareDatabase(wfId);
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\externalVariables\\correlationId.bpmn");
        var corrId = Guid.NewGuid().ToString();

        var inst = await TestEngine.SimpleRun(wfId, xaml, null, corrId);

        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);


        var res = await _workflowEngine.LoadInstanceRaw(inst.Id);
        var dm = await _dbContext.LoadModelAsync(null, "a2wf_test.[Instance.External.Load]", new { InstanceId = inst.Id });

        Assert.AreEqual(corrId, res.CorrelationId);
        Assert.AreEqual(corrId, dm.Eval<String>("Instance.CorrelationId"));
        Assert.AreEqual(corrId, res.State?.Eval<String>("Variables.Process_1.ModelId"));

        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(3, log!.Length);
        Assert.AreEqual("start|task|end", String.Join('|', log));
    }

    [TestMethod]
    public async Task BigintCorrelationId()
    {
        String wfId = "createCorrelationId";

        await TestEngine.PrepareDatabase(wfId);
        var xaml = File.ReadAllText("..\\..\\..\\TestFiles\\externalVariables\\bigint_correlationId.bpmn");
        String corrId = "224499";

        var inst = await TestEngine.SimpleRun(wfId, xaml, null, corrId);

        Assert.AreEqual(WorkflowExecutionStatus.Idle, inst.ExecutionStatus);


        var res = await _workflowEngine.LoadInstanceRaw(inst.Id);
        var dm = await _dbContext.LoadModelAsync(null, "a2wf_test.[Instance.External.Load]", new { InstanceId = inst.Id });

        Assert.AreEqual(corrId, res.CorrelationId);
        Assert.AreEqual(corrId, dm.Eval<String>("Instance.CorrelationId"));
        Assert.AreEqual(corrId, res.State?.Eval<Double>("Variables.Collaboration1.DocId").ToString());

        var log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(1, log!.Length);
        Assert.AreEqual("start", String.Join('|', log));


        inst = await _workflowEngine.ResumeAsync(inst.Id, "Bookmark1", null);
        Assert.AreEqual(WorkflowExecutionStatus.Complete, inst.ExecutionStatus);
        log = inst.Result?.GetNotNull<Object[]>("log");
        Assert.IsNotNull(log);
        Assert.AreEqual(3, log!.Length);
        Assert.AreEqual("start|task|end", String.Join('|', log));
    }
}

