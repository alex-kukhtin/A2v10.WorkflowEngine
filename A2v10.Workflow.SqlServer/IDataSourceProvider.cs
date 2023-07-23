// Copyright © 2023 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;

namespace A2v10.Workflow.SqlServer;

public interface IDataSourceProvider
{
    String? DataSource { get; }
    void SetIdentityParams(ExpandoObject prms);
}
