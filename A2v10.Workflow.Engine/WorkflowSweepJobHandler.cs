// Copyright © 2021-2026 Oleksandr Kukhtin. All rights reserved.

using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using A2v10.Scheduling.Infrastructure;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Engine;

public class WorkflowSweepJobHandler(IWorkflowEngine _engine, ILogger<WorkflowSweepJobHandler> _logger) : IScheduledJob
{
    public async Task ExecuteAsync(ScheduledJobInfo info)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Execute WorkflowSweep at {Time}", DateTime.Now);
        try
        {
            await _engine.ProcessSweep();
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError("Error in WorkflowSweep: {Exception}", ex);
        }
    }
}
