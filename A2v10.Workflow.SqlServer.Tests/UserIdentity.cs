// Copyright © 2021 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using System;

namespace A2v10.Workflow.SqlServer.Tests
{
    public class UserIdentity : IDbIdentity
    {
        public Int64? UserId => null;
        public Int32? TenantId => null;
        public String? Segment => null;
    }
}
