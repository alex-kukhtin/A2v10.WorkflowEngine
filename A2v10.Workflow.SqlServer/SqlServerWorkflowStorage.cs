// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.SqlServer;

public class InputVariable(IVariable var)
{
    public String Name { get; } = var.Name;
    public String Type { get; } = var.Type.ToString().ToLowerInvariant();
    public String? Value { get; } = var.Value;
}
public class SqlServerWorkflowStorage(IDbContext dbContext, ISerializer serializer, IDataSourceProvider dataSourceProvider) : IWorkflowStorage
{
    private readonly IDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly ISerializer _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    private readonly IDataSourceProvider _dataSourceProvider = dataSourceProvider ?? throw new ArgumentNullException(nameof(dataSourceProvider));

    private String? DataSource => _dataSourceProvider.DataSource;

    public Task<ExpandoObject?> LoadWorkflowAsync(IWorkflowIdentity identity)
    {
        var prms = new ExpandoObject()
        {
            { "Id", identity.Id },
            { "Version", identity.Version }
        };
        _dataSourceProvider.SetIdentityParams(prms);
        return _dbContext.ReadExpandoAsync(DataSource, $"{SqlDefinitions.SqlSchema}.[Workflow.Load]", prms);
    }

    public async Task<IWorkflow> LoadAsync(IWorkflowIdentity identity)
    {
        var eo = await LoadWorkflowAsync(identity) 
            ?? throw new SqlServerStorageException($"Workflow not found. (Id:'{identity.Id}', Version:{identity.Version})");
        var wf = new WorkflowElement(
            new WorkflowIdentity(
                eo.GetNotNull<String>("Id"),
                eo.Get<Int32>("Version")
            ),
            _serializer.DeserializeActitity(eo.GetNotNull<String>("Text"), eo.GetNotNull<String>("Format"))
        );
        return wf;
    }

    public IActivity LoadFromBody(String body, String format)
    {
        var result = _serializer.DeserializeActitity(body, format)
            ?? throw new SqlServerStorageException("LoadFromBody failed");
        return result.Activity;
    }

    public async Task<String> LoadSourceAsync(IWorkflowIdentity identity)
    {
        var eo = await LoadWorkflowAsync(identity) 
            ?? throw new SqlServerStorageException($"LoadSource. Workflow not found. (Id:'{identity.Id}', Version:{identity.Version})");
        return eo.Get<String>("Text") ?? throw new SqlServerStorageException("Load source failed");
    }

    public async Task<IWorkflowIdentity> PublishAsync(String id, String text, String format)
    {
        var prms = new ExpandoObject() {
            { "Id", id },
            { "Format", format },
            { "Text", text }
        };
        _dataSourceProvider.SetIdentityParams(prms);
        var res = await _dbContext.ReadExpandoAsync(DataSource, $"{SqlDefinitions.SqlSchema}.[Workflow.Publish]", prms) 
            ?? throw new WorkflowException("Publish failed");
        return new WorkflowIdentity(
            res.GetNotNull<String>("Id"),
            res.Get<Int32>("Version")
        );
    }

    public async Task<IWorkflowIdentity> PublishAsync(IWorkflowCatalog catalog, String id)
    {
        var prms = new ExpandoObject() {
            { "Id", id }
        };
        _dataSourceProvider.SetIdentityParams(prms);
        var res = await _dbContext.ReadExpandoAsync(DataSource, $"{SqlDefinitions.SqlSchema}.[Catalog.Publish]", prms) 
            ?? throw new SqlServerStorageException($"Publish. Workflow not found. (Id:'{id}')");
        var wfIdentity = new WorkflowIdentity(
            res.GetNotNull<String>("Id"),
            res.Get<Int32>("Version")
        );
        // Save input variables For Workflow
        var wf = await LoadAsync(wfIdentity);

        if (wf.Root is IScoped rootScoped)
        {
            if (rootScoped.Variables != null) {
                var inputVariables = rootScoped.Variables.Where(v => v.IsArgument);
                if (inputVariables.Any())
                {
                    var savePrms = new ExpandoObject()
                    {
                        { "Id", wf.Identity.Id },
                        { "Version", wfIdentity.Version}
                    };
                    await _dbContext.SaveListAsync(DataSource, $"{SqlDefinitions.SqlSchema}.[Workflow.SetArguments]", savePrms, inputVariables.Select(v => new InputVariable(v)));
                }
            }
        }
        return wfIdentity;
    }
}
