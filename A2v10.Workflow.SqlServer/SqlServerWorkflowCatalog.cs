// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.SqlServer
{
	public class SqlServerWorkflowCatalog : IWorkflowCatalog
	{
		private readonly IDbContext _dbContext;

		public SqlServerWorkflowCatalog(IDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public Task<WorkflowElem> LoadBodyAsync(String id)
		{
			return _dbContext.LoadAsync<WorkflowElem>(null, $"{SqlDefinitions.SqlSchema}.[Catalog.Load]", new { Id = id });
		}

		public Task<WorkflowThumbElem> LoadThumbAsync(string id)
		{
			throw new NotImplementedException();
		}

		public Task SaveAsync(IWorkflowDescriptor workflow)
		{
			using var sha = SHA256.Create();
			var hash = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(workflow.Body)));

			var eo = new ExpandoObject();
			eo.Set("Id", workflow.Id);
			eo.Set("Body", workflow.Body);
			eo.Set("Format", workflow.Format);
			eo.Set("ThumbFormat", workflow.ThumbFormat);
			eo.Set("Hash", hash);
			return _dbContext.ExecuteExpandoAsync(null, $"{SqlDefinitions.SqlSchema}.[Catalog.Save]", eo);
		}
	}
}
