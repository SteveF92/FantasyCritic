using CSharpFunctionalExtensions;
using FantasyCritic.MySQL;
using FantasyCritic.MySQL.SyncingRepos;
using FantasyCritic.RdsSnapshotManager.Configuration;
using FantasyCritic.RdsSnapshotManager.Infrastructure;

namespace FantasyCritic.RdsSnapshotManager.Services;

public sealed class LocalDatabaseCleanService
{
    private readonly RdsSnapshotManagerOptions _options;
    private readonly DockerMySqlHealthChecker _dockerHealthChecker;
    private readonly MySQLBetaCleaner _cleaner;
    private readonly MySQLFantasyCriticUserStore _localUserStore;

    public LocalDatabaseCleanService(
        RdsSnapshotManagerOptions options,
        DockerMySqlHealthChecker dockerHealthChecker,
        MySQLBetaCleaner cleaner,
        MySQLFantasyCriticUserStore localUserStore)
    {
        _options = options;
        _dockerHealthChecker = dockerHealthChecker;
        _cleaner = cleaner;
        _localUserStore = localUserStore;
    }

    public async Task<Result> Clean(CancellationToken cancellationToken)
    {
        var guard = LocalDatabaseConnectionGuard.ValidateForClean(
            _options.LocalDocker.ConnectionString,
            _options.BetaConnectionString,
            _options.DumpConnectionString);
        if (guard.IsFailure)
        {
            return guard;
        }

        var health = await _dockerHealthChecker.EnsureHealthy(_options.LocalDocker.ContainerName, cancellationToken);
        if (health.IsFailure)
        {
            return health;
        }

        var allUsers = await _localUserStore.GetAllUsers();
        var betaUsers = await _localUserStore.GetUsersInRoleAsync("BetaTester", cancellationToken);
        await _cleaner.CleanEmailsAndPasswords(allUsers, betaUsers);
        return Result.Success();
    }
}
