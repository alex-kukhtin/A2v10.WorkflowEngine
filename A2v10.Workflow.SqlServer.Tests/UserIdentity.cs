// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;

using A2v10.Data.Interfaces;

namespace A2v10.Workflow.SqlServer.Tests
{
	public class UserIdentity : IDbIdentity
	{
		public Int64? UserId => null;
		public Int32? TenantId => null;
		public String Segment => null;
	}
}
