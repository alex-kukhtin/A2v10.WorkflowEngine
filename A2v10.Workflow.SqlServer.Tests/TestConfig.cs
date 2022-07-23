
using Microsoft.Extensions.Configuration;

namespace A2v10.Workflow.SqlServer.Tests;

public static class TestConfig
{
    public static IConfigurationRoot GetRoot()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddUserSecrets("4d3e5faf-b1e2-41fd-90c8-cdbde7661074")
            .AddEnvironmentVariables()
            .Build();
    }
}
