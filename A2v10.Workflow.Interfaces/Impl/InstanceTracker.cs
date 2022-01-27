// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
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

