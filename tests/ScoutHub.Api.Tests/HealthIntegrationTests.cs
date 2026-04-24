using System.Net;

namespace ScoutHub.Api.Tests;

[TestFixture]
public class HealthIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task GetHealthReturnsOkWhenDatabaseIsHealthy()
    {
        var response = await Client.GetAsync("/health");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Is.EqualTo("Healthy"));
    }
}
