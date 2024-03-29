﻿// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.SqlServer
{
    public class SqlTrackRecord : InstanceTracker
    {
        public ActivityTrackAction Action { get; init; }
        public TrackRecordKind Kind { get; init; }
        public Guid InstanceId { get; init; }
        public String? Message { get; init; }
    }
}
