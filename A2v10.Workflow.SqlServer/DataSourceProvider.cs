// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;

using Microsoft.Extensions.Options;

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.SqlServer;

internal class DataSourceProviderScoped(IDbIdentity dbIdentity, IOptions<WorkflowStorageOptions> options) : IDataSourceProvider
{
    private readonly IDbIdentity _dbIdentity = dbIdentity;
    private readonly WorkflowStorageOptions _options = options.Value;

    public String? DataSource => _options.MultiTenant ? _dbIdentity.Segment : _options.DataSource;

    public void SetIdentityParams(ExpandoObject prms)
    {
        if (_dbIdentity.TenantId.HasValue && _options.MultiTenant)
            prms.Set("TenantId", _dbIdentity.TenantId);
        if (_dbIdentity.UserId.HasValue)
            prms.Set("UserId", _dbIdentity.UserId);
    }
}

internal class DataSourceProviderSingleton(IOptions<WorkflowStorageOptions> options) : IDataSourceProvider
{
    private readonly WorkflowStorageOptions _options = options.Value;

    public String? DataSource => _options.DataSource;

    public void SetIdentityParams(ExpandoObject prms)
    {
    }
}
