using CSharpFunctionalExtensions;
using MySqlConnector;

namespace FantasyCritic.RdsSnapshotManager.Infrastructure;

public sealed class DatabaseEmptyChecker
{
    public static string BuildTableCountQuery(string schemaName) =>
        $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{schemaName}' AND table_type = 'BASE TABLE';";

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
