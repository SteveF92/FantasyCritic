using CSharpFunctionalExtensions;
using FantasyCritic.AWS;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Utilities;
using NodaTime;

namespace FantasyCritic.RdsSnapshotManager.Services;

public sealed class SnapshotCreateService
{
    private readonly IClock _clock;

    public SnapshotCreateService(IClock clock)
    {
        _clock = clock;
    }

    public async Task<Result<string>> CreateSnapshot(string instanceName, string? customName, CancellationToken cancellationToken)
    {
        if (customName is not null)
        {
            var validation = RdsSnapshotIdentifierValidator.Validate(customName);
            if (validation.IsFailure)
            {
                return Result.Failure<string>(validation.Error);
            }
        }

        var rdsManager = new RDSManager(instanceName);
        string snapshotId = await rdsManager.SnapshotRDS(_clock.GetCurrentInstant(), customName);
        await WaitUntilAvailable(rdsManager, snapshotId, cancellationToken);
        return Result.Success(snapshotId);
    }

    private async Task WaitUntilAvailable(IRDSManager rdsManager, string snapshotId, CancellationToken cancellationToken)
    {
        var timeout = Duration.FromMinutes(30);
        var deadline = _clock.GetCurrentInstant().Plus(timeout);
        while (_clock.GetCurrentInstant() < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var snaps = await rdsManager.GetRecentSnapshots();
            var match = snaps.SingleOrDefault(s => s.SnapshotName == snapshotId);
            if (match is { Status: "available", Percent: 100 })
            {
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
        }

        throw new TimeoutException($"Snapshot {snapshotId} did not become available within {timeout}.");
    }
}
