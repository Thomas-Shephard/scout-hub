var builder = WebApplication.CreateBuilder(args);

IConfigurationRoot configurationRoot = builder.Configuration;
var allowedOrigins = ResolveAllowedOrigins(configurationRoot)
    ?? throw new InvalidOperationException("Section 'AllowedOrigins' not found.");

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."));

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging();
app.UseCors("DefaultPolicy");

app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();
return;

static string[]? ResolveAllowedOrigins(IConfigurationRoot configurationRoot)
{
    foreach (var provider in configurationRoot.Providers.Reverse())
    {
        if (provider.TryGet("AllowedOrigins", out var sectionValue) && sectionValue == string.Empty)
        {
            return [];
        }

        if (provider.GetChildKeys([], "AllowedOrigins").Any())
        {
            return configurationRoot.GetSection("AllowedOrigins").Get<string[]>();
        }
    }

    return null;
}
