using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyCritic.Lib.Utilities
{
    public class DatabaseSnapshotInfo
    {
        public DatabaseSnapshotInfo(string snapshotName, Instant creationTime, int percent, string status)
        {
            SnapshotName = snapshotName;
            CreationTime = creationTime;
            Percent = percent;
            Status = status;
        }

        public string SnapshotName { get; }
        public Instant CreationTime { get; }
        public int Percent { get; }
        public string Status { get; }
    }
}
