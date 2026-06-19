using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Utilities;
using NodaTime;

namespace FantasyCritic.RdsSnapshotManager.Services;

public sealed class SnapshotCreateService
{
    private readonly IRDSManager _rdsManager;
    private readonly IClock _clock;

    public SnapshotCreateService(IRDSManager rdsManager, IClock clock)
    {
        _rdsManager = rdsManager;
        _clock = clock;
    }

    public async Task<Result<string>> CreateSnapshot(string? customName, CancellationToken cancellationToken)
    {
        if (customName is not null)
        {
            var validation = RdsSnapshotIdentifierValidator.Validate(customName);
            if (validation.IsFailure)
            {
                return Result.Failure<string>(validation.Error);
            }
        }

        string snapshotId = await _rdsManager.SnapshotRDS(_clock.GetCurrentInstant(), customName);
        await WaitUntilAvailable(snapshotId, cancellationToken);
        return Result.Success(snapshotId);
    }

    private async Task WaitUntilAvailable(string snapshotId, CancellationToken cancellationToken)
    {
        var timeout = Duration.FromMinutes(30);
        var deadline = _clock.GetCurrentInstant().Plus(timeout);
        while (_clock.GetCurrentInstant() < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var snaps = await _rdsManager.GetRecentSnapshots();
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
