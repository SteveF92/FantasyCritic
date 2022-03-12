using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Interfaces;

public interface IRDSManager
{
    Task SnapshotRDS(Instant snapshotTime);
    Task<IReadOnlyList<DatabaseSnapshotInfo>> GetRecentSnapshots();
}
