// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Dynamic;

namespace A2v10.Workflow.Tracker;
public abstract class TrackRecord : ITrackRecord
{
    public DateTime EventTime { get; }
    public Int32 RecordNumber { get; set; }

    protected String? Message { get; init; }

    public TrackRecord()
    {
        EventTime = DateTime.UtcNow;
    }

    public override String? ToString()
    {
        return Message;
    }

    public virtual ExpandoObject ToExpandoObject(Int32 no)
    {
        return CreateExpando(no);
    }

    protected ExpandoObject CreateExpando(Int32 no)
    {
        var eo = new ExpandoObject();
        eo.Set("RecordNumber", no);
        eo.Set("EventTime", EventTime);
        eo.Set("Message", Message);
        return eo;
    }
}

