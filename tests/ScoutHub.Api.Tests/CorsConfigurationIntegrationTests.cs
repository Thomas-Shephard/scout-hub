using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;

namespace ScoutHub.Api.Tests;

[TestFixture]
public sealed class CorsConfigurationIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task EmptyAllowedOriginsDoesNotPreventApplicationStartup()
    {
        await using var factory = CreateFactory([]);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/missing");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task EmptyAllowedOriginsDoesNotAddCorsHeaders()
    {
        await using var factory = CreateFactory([]);
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/missing");
        request.Headers.Add("Origin", "http://localhost:3000");

        using var response = await client.SendAsync(request);

        Assert.That(response.Headers.Contains("Access-Control-Allow-Origin"), Is.False);
    }

    [Test]
    public async Task ConfiguredOriginAddsCorsHeaders()
    {
        const string origin = "http://localhost:3000";

        await using var factory = CreateFactory([origin]);
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/missing");
        request.Headers.Add("Origin", origin);

        using var response = await client.SendAsync(request);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values), Is.True);
            Assert.That(values, Is.EquivalentTo([origin]));
        }
    }

    private WebApplicationFactory<Program> CreateFactory(string[] allowedOrigins)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("ConnectionStrings:DefaultConnection", ConnectionString);

            for (var index = 0; index < allowedOrigins.Length; index++)
            {
                builder.UseSetting($"AllowedOrigins:{index}", allowedOrigins[index]);
            }
        });
    }
}
