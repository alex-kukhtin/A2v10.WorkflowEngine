// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;
using System.Dynamic;

namespace A2v10.Workflow.SqlServer;
public static class DbIdentityHelpers
{
    public static void SetIdentityParams(this IDbIdentity dbIdentity, ExpandoObject eo)
    {
        if (dbIdentity.TenantId.HasValue)
            eo.Set("TenantId", dbIdentity.TenantId);
        if (dbIdentity.UserId.HasValue)
            eo.Set("UserId", dbIdentity.UserId);
    }
}

