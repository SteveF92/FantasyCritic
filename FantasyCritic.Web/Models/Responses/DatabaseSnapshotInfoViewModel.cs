using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Utilities;
using NodaTime;

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
