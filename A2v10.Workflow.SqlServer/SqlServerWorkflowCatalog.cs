// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;
using System.Threading.Tasks;

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.SqlServer;
public class SqlServerWorkflowCatalog(IDbContext dbContext, IDataSourceProvider dataSourceProvider) : IWorkflowCatalog
{
    private readonly IDbContext _dbContext = dbContext;
    private readonly IDataSourceProvider _dataSourceProvider = dataSourceProvider;

    private String? DataSource => _dataSourceProvider.DataSource;

    public async Task<WorkflowElem> LoadBodyAsync(String id)
    {
        var prms = new ExpandoObject()
        {
            {"Id", id }
        };
        _dataSourceProvider.SetIdentityParams(prms);
        return await _dbContext.LoadAsync<WorkflowElem>(DataSource, $"{SqlDefinitions.SqlSchema}.[Catalog.Load]", prms)
            ?? throw new WorkflowException("Load body failed");

    }

    public Task<WorkflowThumbElem> LoadThumbAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(IWorkflowDescriptor workflow)
    {
        var prms = new ExpandoObject()
        {
            { "Id", workflow.Id },
            { "Body", workflow.Body},
            { "Key", workflow.Key},
            { "Format", workflow.Format},
            { "ThumbFormat", workflow.ThumbFormat }
        };
        _dataSourceProvider.SetIdentityParams(prms);
        return _dbContext.ExecuteExpandoAsync(DataSource, $"{SqlDefinitions.SqlSchema}.[Catalog.Save]", prms);
    }
}

