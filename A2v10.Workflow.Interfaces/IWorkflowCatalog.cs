// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.IO;

namespace A2v10.Workflow.Interfaces;
public interface IWorkflowDescriptor
{
    String Id { get; }
    String Format { get; }
    String Body { get; }

    String? ThumbFormat { get; }
    Stream? Thumb { get; }
}

public record WorkflowDescriptor(String Id, String Body, String Format = "xaml") : IWorkflowDescriptor
{
    public String? ThumbFormat { get; init; }
    public Stream? Thumb { get; init; }
}

public record WorkflowElem(String Body, String Format);

public record WorkflowThumbElem(Stream Thumb, String Format);

public interface IWorkflowCatalog
{
    Task<WorkflowElem> LoadBodyAsync(String id);
    Task<WorkflowThumbElem> LoadThumbAsync(String id);
    Task SaveAsync(IWorkflowDescriptor workflow);
}

