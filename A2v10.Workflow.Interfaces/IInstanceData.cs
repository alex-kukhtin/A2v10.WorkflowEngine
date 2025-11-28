// Copyright © 2020-2022 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.Workflow.Interfaces;
public enum DeferredElementType
{
    Sql
}

public record DeferredElement(DeferredElementType Type, String Name, ExpandoObject? Parameters, String Refer);

public record DeferredInboxes(List<ExpandoObject> InboxCreate, List<Guid> InboxRemove);

public interface IInstanceData
{
    ExpandoObject? ExternalVariables { get; }
    List<Object>? ExternalBookmarks { get; }
    List<Object>? TrackRecords { get; }
    List<Object>? ExternalEvents { get; }

    List<DeferredElement>? Deferred { get; }
    DeferredInboxes? Inboxes { get; }
    List<ExpandoObject>? UserTrack { get; }
    Boolean HasBatches { get; }
}

