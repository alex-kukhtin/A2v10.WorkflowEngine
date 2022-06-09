// Copyright © 2022 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.SqlServer;

internal record VersionInfo
{
	public Int32 Version { get; set; }
}

public class SqlServerStorageVersion : IWorkflowStorageVersion
{
	private const Int32 REQUIRED_VERSION = 8091;

	private readonly IDbContext _dbContext;
	public SqlServerStorageVersion(IDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public WorkflowStorageVersion GetVersion()
	{
		var ver = _dbContext.Load<VersionInfo>(null, "a2wf.[Version.Get]");
		return new WorkflowStorageVersion(Valid: ver?.Version == REQUIRED_VERSION,
			Required: REQUIRED_VERSION, Actual: ver?.Version ?? 0);
	}
}
