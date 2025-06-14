// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

using System.IO;

namespace A2v10.Workflow.Interfaces;
public interface IWorkflowDescriptor
{
    String Id { get; }
    String Format { get; }
    String Body { get; }

    String? ThumbFormat { get; }
    Stream? Thumb { get; }
    String? Key { get; }
}

public record WorkflowDescriptor(String Id, String Body, String Format = "xaml") : IWorkflowDescriptor
{
    public String? ThumbFormat { get; init; }
    public Stream? Thumb { get; init; }
    public String? Key { get; init; }   
}

public record WorkflowElem
{
    public WorkflowElem()
    {
    }
    public WorkflowElem(String body, String format = "xaml")
    {
        Body = body;
        Format = format;
    }
    public String Body { get; set; } = String.Empty;
    public String Format { get; set; } = "xaml";
}

public record WorkflowThumbElem(Stream Thumb, String Format);

public interface IWorkflowCatalog
{
    Task<WorkflowElem> LoadBodyAsync(String id);
    Task<WorkflowThumbElem> LoadThumbAsync(String id);
    Task SaveAsync(IWorkflowDescriptor workflow);
}

