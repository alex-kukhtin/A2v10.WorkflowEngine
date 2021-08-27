// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
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

		public SqlServerWorkflowStorage(IDbContext dbContext, ISerializer serializer)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
		}

		public Task<ExpandoObject> LoadWorkflowAsync(IWorkflowIdentity identity)
		{
			var prms = new ExpandoObject();
			prms.TryAdd("Id", identity.Id);
			prms.TryAdd("Version", identity.Version);

			return _dbContext.ReadExpandoAsync(null, $"{SqlDefinitions.SqlSchema}.[Workflow.Load]", prms);
		}

		public async Task<IWorkflow> LoadAsync(IWorkflowIdentity identity)
		{
			var eo = await LoadWorkflowAsync(identity);
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
			return eo.Get<String>("Text");
		}

		public async Task<IWorkflowIdentity> PublishAsync(String id, String text, String format)
		{
			var prms = new ExpandoObject();
			prms.Set("Id", id);
			prms.Set("Format", format);
			prms.Set("Text", text);
			var res = await _dbContext.ReadExpandoAsync(null, $"{SqlDefinitions.SqlSchema}.[Workflow.Publish]", prms);

			return new WorkflowIdentity()
			{
				Id = res.Get<String>("Id"),
				Version = res.Get<Int32>("Version")
			};
		}

		public async Task<IWorkflowIdentity> PublishAsync(IWorkflowCatalog catalog, String id)
		{
			if (catalog is SqlServerWorkflowCatalog)
			{
				var prms = new ExpandoObject();
				prms.Set("Id", id);

				var res = await _dbContext.ReadExpandoAsync(null, $"{SqlDefinitions.SqlSchema}.[Catalog.Publish]", prms);

				return new WorkflowIdentity()
				{
					Id = res.Get<String>("Id"),
					Version = res.Get<Int32>("Version")
				};
			}
			else
			{
				var wf = await catalog.LoadBodyAsync(id);
				return await PublishAsync(id, wf.Body, wf.Format);
			}
		}
	}
}