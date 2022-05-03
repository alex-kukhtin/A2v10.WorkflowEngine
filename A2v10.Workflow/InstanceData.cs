// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;
using System.Dynamic;

namespace A2v10.Workflow;

public class InstanceData : IInstanceData
{
    public ExpandoObject? ExternalVariables { get; init; }
    public List<Object>? ExternalBookmarks { get; init; }
    public List<Object>? ExternalEvents { get; init; }
    public List<Object>? TrackRecords { get; init; }
    public List<DeferredElement>? Deferred { get; init; }
    public DeferredInboxes? Inboxes { get; init; }
    public Boolean HasBatches => Deferred != null || Inboxes != null;
}

