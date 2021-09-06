// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;

using Microsoft.Extensions.DependencyInjection;

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.SqlServer
{
	public class ScriptNativeDatabase : IInjectable
	{
		private IDbContext _dbContext;

		public void Inject(IServiceProvider serviceProvider)
		{
			_dbContext = serviceProvider.GetService<IDbContext>() ?? throw new NullReferenceException("DbContext");
		}

#pragma warning disable IDE1006 // Naming Styles
		public ExpandoObject loadModel(String procedure, ExpandoObject prms = null, ExpandoObject opts = null)
#pragma warning restore IDE1006 // Naming Styles
		{
			String source = null;
			if (opts != null)
				source = opts.Get<String>("source");
			var dm = _dbContext.LoadModel(source, procedure, prms);
			return dm.Root;
		}
	}
}
