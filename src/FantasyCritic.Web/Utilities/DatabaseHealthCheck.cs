using FantasyCritic.Lib.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySqlConnector;

namespace FantasyCritic.Web.Utilities;

/// <summary>
/// Readiness check: confirms the app can reach MySQL. Deliberately excluded from the plain
/// /health liveness probe, so that a database blip never pulls the process out of a load
/// balancer's rotation or fails a deploy that is otherwise fine.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    public const string ReadyTag = "ready";

    private readonly RepositoryConfiguration _repositoryConfiguration;

    public DatabaseHealthCheck(RepositoryConfiguration repositoryConfiguration)
    {
        _repositoryConfiguration = repositoryConfiguration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new MySqlConnection(_repositoryConfiguration.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1;";
            await command.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Could not query the database.", exception);
        }
    }
}
