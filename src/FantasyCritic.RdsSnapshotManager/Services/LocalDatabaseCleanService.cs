using CSharpFunctionalExtensions;
using FantasyCritic.MySQL;
using FantasyCritic.MySQL.SyncingRepos;
using FantasyCritic.RdsSnapshotManager.Configuration;
using FantasyCritic.RdsSnapshotManager.Infrastructure;
using Serilog;

namespace FantasyCritic.RdsSnapshotManager.Services;

public sealed class LocalDatabaseCleanService
{
    private readonly RdsSnapshotManagerOptions _options;
    private readonly DockerMySqlHealthChecker _dockerHealthChecker;
    private readonly MySQLBetaCleaner _cleaner;
    private readonly MySQLFantasyCriticUserStore _localUserStore;
    private readonly MySQLLocalSetupSyncer _localSetupSyncer;

    public LocalDatabaseCleanService(
        RdsSnapshotManagerOptions options,
        DockerMySqlHealthChecker dockerHealthChecker,
        MySQLBetaCleaner cleaner,
        MySQLFantasyCriticUserStore localUserStore,
        MySQLLocalSetupSyncer localSetupSyncer)
    {
        _options = options;
        _dockerHealthChecker = dockerHealthChecker;
        _cleaner = cleaner;
        _localUserStore = localUserStore;
        _localSetupSyncer = localSetupSyncer;
    }

    public async Task<Result> Clean(CancellationToken cancellationToken)
    {
        string snapshotConnectionString = LocalSnapshotConnectionString.BuildSnapshotConnectionString(
            _options.LocalDocker.ConnectionString);

        var guard = LocalDatabaseConnectionGuard.ValidateForClean(
            snapshotConnectionString,
            _options.RdsInstances.Values.Select(i => i.ConnectionString));
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

        //A snapshot taken before production had the AllowSignInAs migration has no column to set. That leaves sign-in-as off, which is safe.
        var allowSignInAs = await _localSetupSyncer.AllowSignInAs();
        if (allowSignInAs.IsFailure)
        {
            Log.Warning(allowSignInAs.Error);
        }

        return Result.Success();
    }
}
