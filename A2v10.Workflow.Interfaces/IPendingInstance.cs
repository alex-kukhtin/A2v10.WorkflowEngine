// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public interface IPendingInstance
{
    Guid InstanceId { get; }
    IEnumerable<String> EventKeys { get; }
}

