using System.Net;

namespace ScoutHub.Api.Tests;

[TestFixture]
public sealed class CorsConfigurationIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task EmptyAllowedOriginsDoesNotPreventApplicationStartup()
    {
        await using var factory = CreateFactory(allowedOrigins: []);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/missing");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task EmptyAllowedOriginsDoesNotAddCorsHeaders()
    {
        await using var factory = CreateFactory(allowedOrigins: []);
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/missing");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        using var response = await client.SendAsync(request);

        Assert.That(response.Headers.Contains("Access-Control-Allow-Origin"), Is.False);
    }

    [Test]
    public async Task EmptyAllowedOriginsOverrideClearsDevelopmentCorsOrigins()
    {
        await using var factory = CreateFactory(environment: "Development", allowedOrigins: []);
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/missing");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        using var response = await client.SendAsync(request);

        Assert.That(response.Headers.Contains("Access-Control-Allow-Origin"), Is.False);
    }

    [Test]
    public async Task DevelopmentEnvironmentAllowsConfiguredOrigin()
    {
        const string origin = "http://localhost:5173";

        await using var factory = CreateFactory(environment: "Development");
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");

        using var response = await client.SendAsync(request);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values), Is.True);
            Assert.That(values, Is.EquivalentTo([origin]));
            Assert.That(response.Headers.TryGetValues("Access-Control-Allow-Methods", out var methods), Is.True);
            Assert.That(methods, Has.Some.EqualTo("GET"));
        }
    }
}
