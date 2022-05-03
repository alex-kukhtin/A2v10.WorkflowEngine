// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public interface IWorkflowStorage
{
    Task<IWorkflow> LoadAsync(IWorkflowIdentity identity);
    Task<String> LoadSourceAsync(IWorkflowIdentity identity);
    Task<IWorkflowIdentity> PublishAsync(IWorkflowCatalog catalog, String id);
}

