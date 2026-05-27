// Copyright © 2020-2026 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;

public sealed class WorkflowException(String message) : Exception(message) {}

public sealed class InstanceBusyException(String message) : Exception(message) {}

public sealed class InstanceNotFoundException(String message) : Exception(message) {}

public sealed class InstanceHaltedException(String message) : Exception(message) {}
