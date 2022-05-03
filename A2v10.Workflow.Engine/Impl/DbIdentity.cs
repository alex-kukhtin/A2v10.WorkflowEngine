// Copyright © 2021 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;

namespace A2v10.WorkflowEngine;
public class DbIdentity : IDbIdentity
{
    public Int64? UserId => null;
    public Int32? TenantId => null;
    public String? Segment => null;
}

