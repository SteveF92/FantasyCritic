using CSharpFunctionalExtensions;
using FantasyCritic.AWS;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.MySQL;
using FantasyCritic.MySQL.SyncingRepos;
using FantasyCritic.RdsSnapshotManager.Configuration;
using NodaTime;

namespace FantasyCritic.RdsSnapshotManager.Services;

public sealed class RestoreSnapshotService
{
    private readonly RdsRestoreService _restoreService;
    private readonly RdsSnapshotManagerOptions _options;
    private readonly IClock _clock;

    public RestoreSnapshotService(RdsRestoreService restoreService, RdsSnapshotManagerOptions options, IClock clock)
    {
        _restoreService = restoreService;
        _options = options;
        _clock = clock;
    }

    public async Task<Result> Restore(string snapshotIdentifier, string destinationInstanceKey, CancellationToken cancellationToken)
    {
        var destinationResult = RdsInstanceLookup.TryResolveWriteEnabled(_options.RdsInstances, destinationInstanceKey);
        if (destinationResult.IsFailure)
        {
            return Result.Failure(destinationResult.Error);
        }

        var destination = destinationResult.Value;
        await _restoreService.CopySnapshotToInstance(snapshotIdentifier, destination.InstanceName);

        var repoConfig = new RepositoryConfiguration(destination.ConnectionString, _clock);
        var userStore = new MySQLFantasyCriticUserStore(repoConfig);
        var cleaner = new MySQLBetaCleaner(destination.ConnectionString);

        var allUsers = await userStore.GetAllUsers();
        var betaUsers = await userStore.GetUsersInRoleAsync("BetaTester", cancellationToken);
        await cleaner.CleanEmailsAndPasswords(allUsers, betaUsers);
        return Result.Success();
    }
}
