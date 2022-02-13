
using System;
using System.Dynamic;
using Microsoft.Extensions.DependencyInjection;

using A2v10.Data.Interfaces;
using A2v10.Workflow.Interfaces;


namespace A2v10.Workflow.SqlServer.Tests;
public class TestExternalNatvieType : IInjectable
{

    private IDbContext? _dbContext;

    public void Inject(IServiceProvider serviceProvider)
    {
        _dbContext = serviceProvider.GetRequiredService<IDbContext>() ?? throw new NullReferenceException("DbContext");
    }

#pragma warning disable IDE1006 // Naming Styles
    public ExpandoObject invoke(ExpandoObject arg)
#pragma warning restore IDE1006 // Naming Styles
    {
        if (_dbContext == null)
            throw new NullReferenceException("DbContext");
        var res = arg.Clone();
        res.Set("success", true);
        return res;
    }
}

