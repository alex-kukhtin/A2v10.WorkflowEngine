// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	public class InstanceTracker : ITracker
	{
		private readonly List<ITrackRecord> _records;

		public InstanceTracker()
		{
			_records = new List<ITrackRecord>();
		}

		public List<ITrackRecord> Records => _records;

		public void Start()
		{
			_records.Clear();
		}

		public void Stop()
		{
		}

		public void Track(ITrackRecord record)
		{
			_records.Add(record);
		}
	}
}
