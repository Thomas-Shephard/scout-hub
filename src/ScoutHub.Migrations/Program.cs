using System.Reflection;
using DbUp;

var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? throw new InvalidOperationException("The 'CONNECTION_STRING' environment variable is not set.");

EnsureDatabase.For.PostgresqlDatabase(connectionString);

var upgrader = DeployChanges.To
    .PostgresqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    await Console.Error.WriteLineAsync("Migration failed:");
    await Console.Error.WriteLineAsync(result.Error.ToString());
    return -1;
}

await Console.Out.WriteLineAsync("Migrations completed successfully.");
return 0;
