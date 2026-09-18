using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal static class DatabaseSnapshotJobUtilities
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);
    private static readonly Duration Timeout = Duration.FromMinutes(30);

    //RDS accepts a snapshot request long before the snapshot is usable, and doesn't document when during "creating" the data is captured.
    //So a job that takes a snapshot waits for "available"; otherwise a completed job wouldn't mean the snapshot exists.
    //statusPrefix carries the earlier steps of a multi-step job, so each progress update still says what already finished.
    public static async Task SnapshotDatabaseAndWait(AdminService adminService, IClock clock, FantasyCriticJobContext context,
        string snapshotName, string statusPrefix, CancellationToken cancellationToken)
    {
        await adminService.StartDatabaseSnapshot(snapshotName, cancellationToken);
        await context.UpdateDetailedStatus($"{statusPrefix}Snapshot {snapshotName} requested.");

        var deadline = clock.GetCurrentInstant().Plus(Timeout);
        while (true)
        {
            await Task.Delay(PollInterval, cancellationToken);
            var snapshot = await adminService.GetDatabaseSnapshot(snapshotName, cancellationToken);
            if (snapshot.Status == "available")
            {
                await context.UpdateDetailedStatus($"{statusPrefix}Snapshot {snapshotName} available.");
                return;
            }

            if (snapshot.Status != "creating")
            {
                throw new InvalidOperationException($"Snapshot {snapshotName} has status '{snapshot.Status}', expected 'creating' or 'available'.");
            }

            if (clock.GetCurrentInstant() > deadline)
            {
                throw new TimeoutException($"Snapshot {snapshotName} was still creating ({snapshot.Percent}%) after {Timeout}.");
            }

            await context.UpdateDetailedStatus($"{statusPrefix}Snapshot {snapshotName} creating: {snapshot.Percent}%.");
        }
    }
}
