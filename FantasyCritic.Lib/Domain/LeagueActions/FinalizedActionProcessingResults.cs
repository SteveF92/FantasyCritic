using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Domain.LeagueActions
{
    public class FinalizedActionProcessingResults
    {
        public FinalizedActionProcessingResults(Guid processSetID, Instant processTime, string processName, ActionProcessingResults results)
        {
            ProcessSetID = processSetID;
            ProcessTime = processTime;
            ProcessName = processName;
            Results = results;
        }

        public Guid ProcessSetID { get; }
        public Instant ProcessTime { get; }
        public string ProcessName { get; }
        public ActionProcessingResults Results { get; }

        public IReadOnlyList<LeagueActionProcessingSet> GetLeagueActionSets()
        {
            var allDrops = Results.SuccessDrops.Concat(Results.FailedDrops);
            var dropsByLeague = allDrops.ToLookup(x => x.Publisher.LeagueYear);

            var allBids = Results.SuccessBids.Select(x => x.PickupBid).Concat(Results.FailedBids.Select(x => x.PickupBid));
            var bidsByLeague = allBids.GroupToDictionary(x => x.Publisher.LeagueYear);

            List<LeagueActionProcessingSet> leagueSets = new List<LeagueActionProcessingSet>();
            var leagueYears = bidsByLeague.Keys.ToList();
            foreach (var leagueYear in leagueYears)
            {
                var dropsForLeague = dropsByLeague[leagueYear];
                var bidsForLeague = bidsByLeague[leagueYear];
                leagueSets.Add(new LeagueActionProcessingSet(leagueYear, ProcessSetID, ProcessTime, ProcessName, dropsForLeague, bidsForLeague));
            }

            return leagueSets;
        }
    }
}
