// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.Workflow.Interfaces;
public interface ITrackRecord
{
    DateTime EventTime { get; }
    ExpandoObject ToExpandoObject(Int32 no);
}

public interface ITracker
{
    void Start();
    void Stop();

    void Track(ITrackRecord record);
    List<ITrackRecord> Records { get; }
}

