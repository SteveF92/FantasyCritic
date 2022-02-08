using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace FantasyCritic.Lib.Domain.LeagueActions
{
    public class LeagueActionProcessingSet
    {
        public LeagueActionProcessingSet(LeagueYear leagueYear, Guid processSetID, Instant processTime, string processName, IEnumerable<DropRequest> drops, IEnumerable<PickupBid> bids)
        {
            LeagueYear = leagueYear;
            ProcessSetID = processSetID;
            ProcessTime = processTime;
            ProcessName = processName;
            Drops = drops.ToList();
            Bids = bids.ToList();
        }

        public LeagueYear LeagueYear { get; }
        public Guid ProcessSetID { get; }
        public Instant ProcessTime { get; }
        public string ProcessName { get; }
        public IReadOnlyList<DropRequest> Drops { get; }
        public IReadOnlyList<PickupBid> Bids { get; }
    }
}
