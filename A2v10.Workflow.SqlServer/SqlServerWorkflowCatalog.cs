// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Dynamic;
using System.Threading.Tasks;

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.SqlServer;
public class SqlServerWorkflowCatalog : IWorkflowCatalog
{
	private readonly IDbContext _dbContext;
	private readonly IDbIdentity _dbIdentity;

	public SqlServerWorkflowCatalog(IDbContext dbContext, IDbIdentity dbIdentity)
	{
		_dbContext = dbContext;
		_dbIdentity = dbIdentity;
	}

	public async Task<WorkflowElem> LoadBodyAsync(String id)
	{
		var prms = new ExpandoObject()
		{
			{"Id", id }
		};
		_dbIdentity.SetIdentityParams(prms);
		return await _dbContext.LoadAsync<WorkflowElem>(null, $"{SqlDefinitions.SqlSchema}.[Catalog.Load]", prms)
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
			{ "Format", workflow.Format},
			{ "ThumbFormat", workflow.ThumbFormat }
		};
		_dbIdentity.SetIdentityParams(prms);
		return _dbContext.ExecuteExpandoAsync(null, $"{SqlDefinitions.SqlSchema}.[Catalog.Save]", prms);
	}
}

