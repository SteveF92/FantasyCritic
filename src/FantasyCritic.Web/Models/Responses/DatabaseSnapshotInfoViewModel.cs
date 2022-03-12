using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Web.Models.Responses
{
    public class DatabaseSnapshotInfoViewModel
    {
        public DatabaseSnapshotInfoViewModel(DatabaseSnapshotInfo domain)
        {
            SnapshotName = domain.SnapshotName;
            CreationTime = domain.CreationTime.InZone(TimeExtensions.EasternTimeZone).LocalDateTime;
            Percent = domain.Percent;
            Status = domain.Status;
        }

        public string SnapshotName { get; }
        public LocalDateTime CreationTime { get; }
        public int Percent { get; }
        public string Status { get; }
    }
}
