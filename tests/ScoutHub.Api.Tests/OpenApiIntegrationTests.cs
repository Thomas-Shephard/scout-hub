using System.Net;

namespace ScoutHub.Api.Tests;

[TestFixture]
public sealed class OpenApiIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task DevelopmentEnvironmentExposesOpenApiDocument()
    {
        await using var factory = CreateFactory(environment: "Development");
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task ProductionEnvironmentDoesNotExposeOpenApiDocument()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
