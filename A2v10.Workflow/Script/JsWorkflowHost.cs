
using Microsoft.Extensions.DependencyInjection;

namespace A2v10.Workflow;

public class JsWorkflowHost(IServiceProvider _serviceProvider)
{
    private IWorkflowStorage wfStorage = _serviceProvider.GetRequiredService<IWorkflowStorage>();
    public String WorkflowByKey(String key)
    {
        var name = wfStorage.GetProcessIdByKey(key);
        return $"bpmn:{name}";
    }
}
