namespace ScoutHub.Api.Middleware;

/// <summary>
/// Catches unhandled exceptions, logs them, and returns a problem details response.
/// </summary>
/// <param name="next">The next middleware in the request pipeline.</param>
/// <param name="logger">The logger used to record unhandled exceptions.</param>
/// <param name="env">The hosting environment used to control error detail output.</param>
public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment env)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Processes the current request and converts unhandled exceptions into problem details responses.
    /// </summary>
    /// <param name="context">The current HTTP request context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = context.Response.StatusCode,
            Title = "An error occurred while processing your request.",
            Detail = env.IsDevelopment() ? exception.ToString() : "An internal server error occurred.",
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problemDetails, SerializerOptions, contentType: "application/problem+json");
    }
}
