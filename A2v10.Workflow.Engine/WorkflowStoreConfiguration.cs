
namespace A2v10.Workflow.Engine;

internal class WorkflowStoreConfiguration
{
    public String? DataSource { get; set; }
    public Boolean MultiTenant { get; set; }

    public const String ConfigurationKey = "Workflow:Store";
}
