using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Utilities;
using NodaTime;

namespace FantasyCritic.Lib.Interfaces
{
    public interface IRDSManager
    {
        Task SnapshotRDS(Instant snapshotTime);
        Task<IReadOnlyList<DatabaseSnapshotInfo>> GetRecentSnapshots();
    }
}
