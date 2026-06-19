using CSharpFunctionalExtensions;
using FantasyCritic.AWS;
using FantasyCritic.MySQL;
using FantasyCritic.MySQL.SyncingRepos;
using FantasyCritic.RdsSnapshotManager.Configuration;

namespace FantasyCritic.RdsSnapshotManager.Services;

public sealed class BetaSyncService
{
    private readonly RdsRestoreService _restoreService;
    private readonly RdsSnapshotManagerOptions _options;
    private readonly MySQLBetaCleaner _cleaner;
    private readonly MySQLFantasyCriticUserStore _userStore;

    public BetaSyncService(
        RdsRestoreService restoreService,
        RdsSnapshotManagerOptions options,
        MySQLBetaCleaner cleaner,
        MySQLFantasyCriticUserStore userStore)
    {
        _restoreService = restoreService;
        _options = options;
        _cleaner = cleaner;
        _userStore = userStore;
    }

    public async Task<Result> Sync(string snapshotIdentifier, CancellationToken cancellationToken)
    {
        await _restoreService.CopySnapshotToInstance(snapshotIdentifier, _options.BetaRdsInstance);
        var allUsers = await _userStore.GetAllUsers();
        var betaUsers = await _userStore.GetUsersInRoleAsync("BetaTester", cancellationToken);
        await _cleaner.CleanEmailsAndPasswords(allUsers, betaUsers);
        return Result.Success();
    }
}
