// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	public class WorkflowDeferred : IDeferredTarget
	{
		private readonly Lazy<List<DeferredElement>> _deferred = new();

		#region IDeferredTarget

		public List<DeferredElement> Deferred => _deferred.IsValueCreated ? _deferred.Value : null;
		public String Refer { get; set; }

		public void AddDeffered(DeferredElement elem)
		{
			_deferred.Value.Add(elem);
		}
		#endregion
	}
}
