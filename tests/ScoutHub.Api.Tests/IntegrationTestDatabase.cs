using Npgsql;
using Testcontainers.PostgreSql;

namespace ScoutHub.Api.Tests;

[SetUpFixture]
public sealed class IntegrationTestDatabaseSetUp
{
    [OneTimeSetUp]
    public Task OneTimeSetUp()
    {
        return IntegrationTestDatabase.InitializeAsync();
    }

    [OneTimeTearDown]
    public Task OneTimeTearDown()
    {
        return IntegrationTestDatabase.DisposeAsync();
    }
}

internal static class IntegrationTestDatabase
{
    private static readonly PostgreSqlContainer Container = new PostgreSqlBuilder("postgres:18-alpine")
        .Build();

    public static string ConnectionString { get; private set; } = string.Empty;

    public static async Task InitializeAsync()
    {
        await Container.StartAsync();
        ConnectionString = Container.GetConnectionString();
    }

    public static async Task ResetAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        const string getTablesSql = """
            SELECT quote_ident(schemaname) || '.' || quote_ident(tablename)
            FROM pg_tables
            WHERE schemaname NOT IN ('pg_catalog', 'information_schema')
            ORDER BY schemaname, tablename;
            """;

        await using var getTablesCommand = new NpgsqlCommand(getTablesSql, connection);
        var tables = new List<string>();

        await using (var reader = await getTablesCommand.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }
        }

        if (tables.Count == 0)
        {
            return;
        }

        var truncateSql = $"TRUNCATE TABLE {string.Join(", ", tables)} RESTART IDENTITY CASCADE;";
        await using var truncateCommand = new NpgsqlCommand(truncateSql, connection);
        await truncateCommand.ExecuteNonQueryAsync();
    }

    public static async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}
