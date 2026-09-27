using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Interfaces;

public interface IRDSManager
{
    Task SnapshotRDS(string snapshotIdentifier, CancellationToken cancellationToken);
    Task<DatabaseSnapshotInfo> GetSnapshot(string snapshotIdentifier, CancellationToken cancellationToken);
    Task<IReadOnlyList<DatabaseSnapshotInfo>> GetRecentSnapshots();
}
