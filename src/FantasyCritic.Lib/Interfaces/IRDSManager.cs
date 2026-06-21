using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Interfaces;

public interface IRDSManager
{
    Task<string> SnapshotRDS(Instant snapshotTime, string? snapshotIdentifier = null);
    Task<IReadOnlyList<DatabaseSnapshotInfo>> GetRecentSnapshots();
}
