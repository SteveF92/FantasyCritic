using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Domain.LeagueActions;

public class FinalizedActionProcessingResults
{
    public FinalizedActionProcessingResults(Guid processSetID, Instant processTime, string processName, ActionProcessingResults results, IEnumerable<SpecialAuction> specialAuctionsProcessed)
    {
        ProcessSetID = processSetID;
        ProcessTime = processTime;
        ProcessName = processName;
        Results = results;
        SpecialAuctionsProcessed = specialAuctionsProcessed.ToList();
    }

    public Guid ProcessSetID { get; }
    public Instant ProcessTime { get; }
    public string ProcessName { get; }
    public ActionProcessingResults Results { get; }
    public IReadOnlyList<SpecialAuction> SpecialAuctionsProcessed { get; }

    public bool IsEmpty()
    {
        if (SpecialAuctionsProcessed.Any())
        {
            return false;
        }

        if (Results.LeagueActions.Any())
        {
            return false;
        }

        return true;
    }

    public IReadOnlyList<LeagueActionProcessingSet> GetLeagueActionSets()
    {
        List<DropRequest> allDrops = new List<DropRequest>();
        foreach (var successDrop in Results.SuccessDrops)
        {
            allDrops.Add(successDrop.ToDropWithSuccess(true, ProcessSetID));
        }
        foreach (var failedDrop in Results.FailedDrops)
        {
            allDrops.Add(failedDrop.ToDropWithSuccess(false, ProcessSetID));
        }
        
        List<PickupBid> allBids = new List<PickupBid>();
        foreach (var successBid in Results.SuccessBids)
        {
            allBids.Add(successBid.ToFlatBid(ProcessSetID));
        }
        foreach (var failedBid in Results.FailedBids)
        {
            allBids.Add(failedBid.ToFlatBid(ProcessSetID));
        }
        
        var bidsByLeague = allBids.GroupToDictionary(x => x.LeagueYear);
        var dropsByLeague = allDrops.ToLookup(x => x.LeagueYear);

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
