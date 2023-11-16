// Copyright © 2020-2023 Oleksnadr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public class InstanceTracker : ITracker
{
    private readonly List<ITrackRecord> _records = [];

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

