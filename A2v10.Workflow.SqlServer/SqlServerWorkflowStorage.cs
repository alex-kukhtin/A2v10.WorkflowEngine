// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.Threading.Tasks;

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.SqlServer
{
	public class SqlServerWorkflowStorage : IWorkflowStorage
	{
		private readonly IDbContext _dbContext;
		private readonly ISerializer _serializer;
		private readonly IDbIdentity _dbIdentity;

		public SqlServerWorkflowStorage(IDbContext dbContext, ISerializer serializer, IDbIdentity dbIdentity)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
			_dbIdentity = dbIdentity ?? throw new ArgumentNullException(nameof(dbIdentity));
		}

		private String DataSource => String.IsNullOrEmpty(_dbIdentity.Segment) ? null : _dbIdentity.Segment;

		public Task<ExpandoObject> LoadWorkflowAsync(IWorkflowIdentity identity)
		{
			var prms = new ExpandoObject()
			{
				{ "Id", identity.Id },
				{ "Version", identity.Version }
			};
			_dbIdentity.SetIdentityParams(prms);
			return _dbContext.ReadExpandoAsync(DataSource, $"{SqlDefinitions.SqlSchema}.[Workflow.Load]", prms);
		}

		public async Task<IWorkflow> LoadAsync(IWorkflowIdentity identity)
		{
			var eo = await LoadWorkflowAsync(identity);
			if (eo == null)
				throw new SqlServerStorageException($"Workflow not found. (Id:'{identity.Id}', Version:{identity.Version})");
			var wf = new Workflow()
			{
				Identity = new WorkflowIdentity()
				{
					Id = eo.Get<String>("Id"),
					Version = eo.Get<Int32>("Version")
				},
				Root = _serializer.DeserializeActitity(eo.Get<String>("Text"), eo.Get<String>("Format"))
			};
			return wf;
		}

		public async Task<String> LoadSourceAsync(IWorkflowIdentity identity)
		{
			var eo = await LoadWorkflowAsync(identity);
			if (eo == null)
				throw new SqlServerStorageException($"LoadSource. Workflow not found. (Id:'{identity.Id}', Version:{identity.Version})");
			return eo.Get<String>("Text");
		}

		public async Task<IWorkflowIdentity> PublishAsync(String id, String text, String format)
		{
			var prms = new ExpandoObject() {
				{ "Id", id },
				{ "Format", format },
				{ "Text", text }
			};
			_dbIdentity.SetIdentityParams(prms);
			var res = await _dbContext.ReadExpandoAsync(DataSource, $"{SqlDefinitions.SqlSchema}.[Workflow.Publish]", prms);

			return new WorkflowIdentity()
			{
				Id = res.Get<String>("Id"),
				Version = res.Get<Int32>("Version")
			};
		}

		public async Task<IWorkflowIdentity> PublishAsync(IWorkflowCatalog catalog, String id)
		{
			var prms = new ExpandoObject() {
				{ "Id", id }
			};
			_dbIdentity.SetIdentityParams(prms);
			var res = await _dbContext.ReadExpandoAsync(DataSource, $"{SqlDefinitions.SqlSchema}.[Catalog.Publish]", prms);

			if (res == null)
				throw new SqlServerStorageException($"Publish. Workflow not found. (Id:'{id}')");


			return new WorkflowIdentity()
			{
				Id = res.Get<String>("Id"),
				Version = res.Get<Int32>("Version")
			};
		}
	}
}