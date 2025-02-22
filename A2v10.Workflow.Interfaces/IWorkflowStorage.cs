// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public interface IWorkflowStorage
{
    Task<IWorkflow> LoadAsync(IWorkflowIdentity identity);
    Task<String> LoadSourceAsync(IWorkflowIdentity identity);
    Task<IWorkflowIdentity> PublishAsync(IWorkflowCatalog catalog, String id);
    IActivity LoadFromBody(String body, String format);
    ExpandoObject LoadPersistentValue(String procedure, Object id);
}

