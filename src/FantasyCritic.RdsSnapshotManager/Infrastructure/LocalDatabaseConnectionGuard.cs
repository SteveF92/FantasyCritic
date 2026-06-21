using CSharpFunctionalExtensions;
using FantasyCritic.RdsSnapshotManager.Configuration;
using MySqlConnector;

namespace FantasyCritic.RdsSnapshotManager.Infrastructure;

public static class LocalDatabaseConnectionGuard
{
    private const uint LocalDockerPort = 3307;

    public static Result ValidateForClean(string localDockerConnectionString, string betaConnectionString, string dumpConnectionString)
    {
        if (string.IsNullOrWhiteSpace(localDockerConnectionString))
        {
            return Result.Failure("Local Docker connection string is not configured.");
        }

        if (ContainsRemoteDatabaseMarker(localDockerConnectionString))
        {
            return Result.Failure("Refusing to clean: local Docker connection string appears to target a remote database.");
        }

        if (string.Equals(localDockerConnectionString, betaConnectionString, StringComparison.Ordinal))
        {
            return Result.Failure("Refusing to clean: local Docker connection string matches the beta connection string.");
        }

        if (string.Equals(localDockerConnectionString, dumpConnectionString, StringComparison.Ordinal))
        {
            return Result.Failure("Refusing to clean: local Docker connection string matches the dump connection string.");
        }

        var builder = new MySqlConnectionStringBuilder(localDockerConnectionString);
        var server = builder.Server.Trim().ToLowerInvariant();

        if (server != "localhost" && server != "127.0.0.1" && server != "::1")
        {
            return Result.Failure($"Refusing to clean: server must be localhost, got '{builder.Server}'.");
        }

        if (builder.Port != LocalDockerPort)
        {
            return Result.Failure($"Refusing to clean: port must be {LocalDockerPort} (local Docker), got {builder.Port}.");
        }

        if (string.Equals(builder.Database, LocalSnapshotDatabaseNames.SeededDatabase, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(
                $"Refusing to clean: database must not be the seeded local database '{LocalSnapshotDatabaseNames.SeededDatabase}'.");
        }

        if (!string.Equals(builder.Database, LocalSnapshotDatabaseNames.SnapshotDatabase, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(
                $"Refusing to clean: database must be '{LocalSnapshotDatabaseNames.SnapshotDatabase}', got '{builder.Database}'.");
        }

        return Result.Success();
    }

    private static bool ContainsRemoteDatabaseMarker(string connectionString)
    {
        return connectionString.Contains("amazonaws.com", StringComparison.OrdinalIgnoreCase)
            || connectionString.Contains(".rds.", StringComparison.OrdinalIgnoreCase);
    }
}
