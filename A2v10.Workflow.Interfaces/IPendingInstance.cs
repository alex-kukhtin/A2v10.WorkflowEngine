// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public interface IPendingInstance
{
    Guid InstanceId { get; }
    IEnumerable<String> EventKeys { get; }
}

public interface IPendingMessage
{
    Int64 Id { get; }
    Guid InstanceId { get; }
    String Message { get; }
}
