// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	public class ConsoleTracker : ITracker
	{
		Int32 _no;

		public ConsoleTracker()
		{
			_no = 1;
		}

		public void Track(ITrackRecord record)
		{
			Console.WriteLine($"{_no++}: {record}");
		}

		public List<ITrackRecord> Records => null;

		public void Start()
		{
			Console.WriteLine($"Start tracking");
		}

		public void Stop()
		{
			Console.WriteLine($"Stop tracking");
		}
	}
}
