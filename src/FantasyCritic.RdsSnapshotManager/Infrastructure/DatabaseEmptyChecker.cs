using CSharpFunctionalExtensions;
using MySqlConnector;

namespace FantasyCritic.RdsSnapshotManager.Infrastructure;

public sealed class DatabaseEmptyChecker
{
    public static string BuildTableCountQuery(string schemaName) =>
        $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{schemaName}' AND table_type = 'BASE TABLE';";

    public static string BuildCreateDatabaseQuery(string schemaName) =>
        $"CREATE DATABASE IF NOT EXISTS `{schemaName.Replace("`", "``")}`;";

    public static string BuildEnsureAppUserGrantsQuery(string schemaName) =>
        $"GRANT EXECUTE, SELECT, SHOW VIEW, DELETE, INSERT, UPDATE, CREATE TEMPORARY TABLES ON `{schemaName.Replace("`", "``")}`.* TO 'fantasycritic'@'%';";

    public async Task<Result> EnsureDatabaseExistsOrFailure(
        string connectionString,
        string schemaName,
        CancellationToken cancellationToken)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString)
        {
            Database = string.Empty
        };

        await using var connection = new MySqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using (var createCommand = new MySqlCommand(BuildCreateDatabaseQuery(schemaName), connection))
        {
            await createCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var grantCommand = new MySqlCommand(BuildEnsureAppUserGrantsQuery(schemaName), connection))
        {
            await grantCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> EnsureEmptyOrFailure(string connectionString, string schemaName, CancellationToken cancellationToken)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(BuildTableCountQuery(schemaName), connection);
        var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        return count == 0
            ? Result.Success()
            : Result.Failure($"Database '{schemaName}' is not empty ({count} tables).");
    }
}
