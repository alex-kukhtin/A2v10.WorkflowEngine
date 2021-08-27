// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Workflow.SqlServer
{
	public sealed class SqlServerStorageException : Exception
	{
		public SqlServerStorageException(String message)
			: base(message)
		{
		}
	}
}
