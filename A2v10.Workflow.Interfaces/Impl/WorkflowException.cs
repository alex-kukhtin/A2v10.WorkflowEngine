// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public sealed class WorkflowException(String message) : Exception(message)
{
}
