// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace A2v10.Workflow.Interfaces
{
	public interface IInstanceStorage
	{
		Task<IInstance> Load(Guid id);

		Task Create(IInstance instance);
		Task Save(IInstance instance);

		Task WriteException(Guid id, Exception ex);

		Task<IEnumerable<IPendingInstance>> GetPendingAsync();
	}
}
