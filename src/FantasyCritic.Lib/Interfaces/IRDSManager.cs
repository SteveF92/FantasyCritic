using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Interfaces;

public interface IRDSManager
{
    Task<string> SnapshotRDS(Instant snapshotTime, string? snapshotIdentifier, CancellationToken cancellationToken);
    Task<DatabaseSnapshotInfo> GetSnapshot(string snapshotIdentifier, CancellationToken cancellationToken);
    Task<IReadOnlyList<DatabaseSnapshotInfo>> GetRecentSnapshots();
}
