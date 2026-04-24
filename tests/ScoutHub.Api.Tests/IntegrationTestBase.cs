using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace ScoutHub.Api.Tests;

public abstract class IntegrationTestBase
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18-alpine")
        .Build();

    protected WebApplicationFactory<Program> Factory { get; private set; }
    protected HttpClient Client { get; private set; }
    protected string ConnectionString => _dbContainer.GetConnectionString();

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        await _dbContainer.StartAsync();

        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", ConnectionString);
        });

        Client = Factory.CreateClient();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }
}
