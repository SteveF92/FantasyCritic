using CSharpFunctionalExtensions;
using FantasyCritic.MySQL;
using FantasyCritic.MySQL.SyncingRepos;
using FantasyCritic.RdsSnapshotManager.Configuration;
using FantasyCritic.RdsSnapshotManager.Infrastructure;

namespace FantasyCritic.RdsSnapshotManager.Services;

public sealed class LocalImportService
{
    private readonly RdsSnapshotManagerOptions _options;
    private readonly DockerMySqlHealthChecker _dockerHealthChecker;
    private readonly DatabaseEmptyChecker _emptyChecker;
    private readonly MysqldumpRunner _mysqldumpRunner;
    private readonly MySQLBetaCleaner _cleaner;
    private readonly MySQLFantasyCriticUserStore _localUserStore;

    public LocalImportService(
        RdsSnapshotManagerOptions options,
        DockerMySqlHealthChecker dockerHealthChecker,
        DatabaseEmptyChecker emptyChecker,
        MysqldumpRunner mysqldumpRunner,
        MySQLBetaCleaner cleaner,
        MySQLFantasyCriticUserStore localUserStore)
    {
        _options = options;
        _dockerHealthChecker = dockerHealthChecker;
        _emptyChecker = emptyChecker;
        _mysqldumpRunner = mysqldumpRunner;
        _cleaner = cleaner;
        _localUserStore = localUserStore;
    }

    public async Task<Result> Import(string gzipFilePath, bool force, CancellationToken cancellationToken)
    {
        string snapshotConnectionString = LocalSnapshotConnectionString.BuildSnapshotConnectionString(
            _options.LocalDocker.ConnectionString);

        var guard = LocalDatabaseConnectionGuard.ValidateForClean(
            snapshotConnectionString,
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

        var ensureDatabase = await _emptyChecker.EnsureDatabaseExistsOrFailure(
            snapshotConnectionString,
            LocalSnapshotDatabaseNames.SnapshotDatabase,
            cancellationToken);
        if (ensureDatabase.IsFailure)
        {
            return ensureDatabase;
        }

        if (!force)
        {
            var empty = await _emptyChecker.EnsureEmptyOrFailure(
                snapshotConnectionString,
                LocalSnapshotDatabaseNames.SnapshotDatabase,
                cancellationToken);
            if (empty.IsFailure)
            {
                return empty;
            }
        }

        var import = await _mysqldumpRunner.ImportGzipFile(snapshotConnectionString, gzipFilePath, cancellationToken);
        if (import.IsFailure)
        {
            return import;
        }

        var allUsers = await _localUserStore.GetAllUsers();
        var betaUsers = await _localUserStore.GetUsersInRoleAsync("BetaTester", cancellationToken);
        await _cleaner.CleanEmailsAndPasswords(allUsers, betaUsers);
        return Result.Success();
    }
}
