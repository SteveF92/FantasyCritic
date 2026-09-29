using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Interfaces;

public interface IRDSManager
{
    //Returns once RDS accepts the request, while the snapshot is still being created. Poll GetSnapshot to know when it's usable.
    Task SnapshotRDS(string snapshotIdentifier, CancellationToken cancellationToken);
    Task<DatabaseSnapshotInfo> GetSnapshot(string snapshotIdentifier, CancellationToken cancellationToken);
    Task<IReadOnlyList<DatabaseSnapshotInfo>> GetRecentSnapshots();
}
