using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ScoutHub.Api.Tests;

public abstract class IntegrationTestBase
{
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    protected WebApplicationFactory<Program> Factory => _factory ??= CreateFactory();
    protected HttpClient Client => _client ??= Factory.CreateClient();
    protected static string ConnectionString => IntegrationTestDatabase.ConnectionString;

    [SetUp]
    public Task SetUp()
    {
        return IntegrationTestDatabase.ResetAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _client?.Dispose();

        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }
    }

    protected WebApplicationFactory<Program> CreateFactory(
        string environment = "Production",
        string[]? allowedOrigins = null,
        Action<IWebHostBuilder>? configure = null)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.UseSetting("ConnectionStrings:DefaultConnection", ConnectionString);

            if (allowedOrigins is [])
            {
                builder.UseSetting("AllowedOrigins", string.Empty);
            }
            else if (allowedOrigins is not null)
            {
                for (var index = 0; index < allowedOrigins.Length; index++)
                {
                    builder.UseSetting($"AllowedOrigins:{index}", allowedOrigins[index]);
                }
            }

            configure?.Invoke(builder);
        });
    }
}
