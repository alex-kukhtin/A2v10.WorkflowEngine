// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.SqlServer;
public sealed class SqlServerStorageException : Exception
{
    public SqlServerStorageException(String message)
        : base(message)
    {
    }
}

