using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace ScoutHub.Api.Tests;

[TestFixture]
public sealed class ExceptionHandlingIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task UnhandledExceptionReturnsProblemDetailsInProduction()
    {
        await using var factory = CreateFactory(
            configure: ConfigureExceptionTestServices);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/test-exceptions/throw");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
        }

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.That(problemDetails, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(problemDetails.Status, Is.EqualTo((int)HttpStatusCode.InternalServerError));
            Assert.That(problemDetails.Title, Is.EqualTo("An error occurred while processing your request."));
            Assert.That(problemDetails.Detail, Is.EqualTo("An internal server error occurred."));
            Assert.That(problemDetails.Instance, Is.EqualTo("/test-exceptions/throw"));
        }
    }

    [Test]
    public async Task UnhandledExceptionReturnsExceptionDetailsInDevelopment()
    {
        await using var factory = CreateFactory(
            environment: "Development",
            configure: ConfigureExceptionTestServices);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/test-exceptions/throw");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problemDetails, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(problemDetails.Detail, Does.Contain(nameof(InvalidOperationException)));
            Assert.That(problemDetails.Detail, Does.Contain("Test exception"));
        }
    }

    private static void ConfigureExceptionTestServices(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddControllers()
                .PartManager.ApplicationParts.Add(new AssemblyPart(typeof(TestExceptionsController).Assembly));
        });
    }
}

[ApiController]
[Route("test-exceptions")]
public sealed class TestExceptionsController : ControllerBase
{
    [HttpGet("throw")]
    public IActionResult Throw()
    {
        throw new InvalidOperationException("Test exception");
    }
}
