
using System.Dynamic;

using Microsoft.Extensions.Options;

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.SqlServer;

internal class DataSourceProviderScoped : IDataSourceProvider
{
    private readonly IDbIdentity _dbIdentity;
    private readonly WorkflowStorageOptions _options;
    public DataSourceProviderScoped(IDbIdentity dbIdentity, IOptions<WorkflowStorageOptions> options)
    {
        _dbIdentity = dbIdentity;   
        _options = options.Value; 
    }
    public String? DataSource => _options.MultiTenant ? _dbIdentity.Segment : _options.DataSource;

    public void SetIdentityParams(ExpandoObject prms)
    {
        if (_dbIdentity.TenantId.HasValue && _options.MultiTenant)
            prms.Set("TenantId", _dbIdentity.TenantId);
        if (_dbIdentity.UserId.HasValue)
            prms.Set("UserId", _dbIdentity.UserId);
    }
}

internal class DataSourceProviderSingleton : IDataSourceProvider
{
    private readonly WorkflowStorageOptions _options;
    public DataSourceProviderSingleton(IOptions<WorkflowStorageOptions> options)
    {
        _options = options.Value;
    }
    public String? DataSource => _options.DataSource;

    public void SetIdentityParams(ExpandoObject prms)
    {
    }
}
