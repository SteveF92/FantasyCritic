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

        public IReadOnlyList<LeagueActionProcessingSet> GetLeagueActionSets(bool dryRun)
        {
            var allDrops = Results.SuccessDrops.Concat(Results.FailedDrops);
            var dropsByLeague = allDrops.ToLookup(x => x.Publisher.LeagueYear);

            List<PickupBid> allBids;
            if (!dryRun)
            {
                allBids = Results.SuccessBids.Select(x => x.PickupBid).Concat(Results.FailedBids.Select(x => x.PickupBid)).ToList();
            }
            else
            {
                allBids = new List<PickupBid>();
                foreach (var successBid in Results.SuccessBids)
                {
                    allBids.Add(successBid.ToFlatBid(ProcessSetID));
                }
                foreach (var failedBid in Results.FailedBids)
                {
                    allBids.Add(failedBid.ToFlatBid(ProcessSetID));
                }
            }
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
