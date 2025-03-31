// Copyright © 2021-2025 Oleksandr Kukhtin. All rights reserved.

using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using A2v10.Scheduling.Infrastructure;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Engine;

public class WorkflowPendingJobHandler(IWorkflowEngine _engine, ILogger<WorkflowPendingJobHandler> _logger) : IScheduledJob
{
    public async Task ExecuteAsync(ScheduledJobInfo info)
    {
        _logger.LogInformation("Execute WorkflowPending at {Time}", DateTime.Now);
        try
        {
            await _engine.ProcessPending();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in WorkflowPending: {Exception}", ex);
            //_engine.WriteCommonException(ex);
        }
    }
}
