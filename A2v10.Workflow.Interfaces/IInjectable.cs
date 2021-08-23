// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Workflow.Interfaces
{
	public interface IInjectable
	{
		public void Inject(IServiceProvider serviceProvider);
	}
}
