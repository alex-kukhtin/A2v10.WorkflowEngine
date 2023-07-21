using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using A2v10.Scheduling.Infrastructure;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Engine;

public class WorkflowPendingJobHandler : IScheduledJob
{
    private readonly IWorkflowEngine _engine;
    public WorkflowPendingJobHandler(IWorkflowEngine engine)
    {
        _engine = engine;
    }
    public async Task ExecuteAsync(ScheduledJobInfo info)
    {
        await _engine.ProcessPending();
    }
}
