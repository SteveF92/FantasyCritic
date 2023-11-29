using FantasyCritic.Lib.Domain.Combinations;

namespace FantasyCritic.Lib.Domain.LeagueActions;

public class ActionProcessingResults
{
    private ActionProcessingResults(IEnumerable<SucceededPickupBid> successBids, IEnumerable<FailedPickupBid> failedBids,
        IEnumerable<DropRequest> successDrops, IEnumerable<DropRequest> failedDrops, IEnumerable<LeagueAction> leagueActions,
        PublisherStateSet publisherStateSet, IEnumerable<PublisherGame> addedPublisherGames, IEnumerable<FormerPublisherGame> removedPublisherGames,
        IEnumerable<LeagueManagerAction> leagueManagerActions)
    {
        SuccessBids = successBids.ToList();
        FailedBids = failedBids.ToList();
        SuccessDrops = successDrops.ToList();
        FailedDrops = failedDrops.ToList();
        LeagueActions = leagueActions.ToList();
        LeagueManagerActions = leagueManagerActions.ToList();
        PublisherStateSet = publisherStateSet;
        AddedPublisherGames = addedPublisherGames.ToList();
        RemovedPublisherGames = removedPublisherGames.ToList();
    }

    public IReadOnlyList<SucceededPickupBid> SuccessBids { get; }
    public IReadOnlyList<FailedPickupBid> FailedBids { get; }
    public IReadOnlyList<DropRequest> SuccessDrops { get; }
    public IReadOnlyList<DropRequest> FailedDrops { get; }
    public IReadOnlyList<LeagueAction> LeagueActions { get; }
    public IReadOnlyList<LeagueManagerAction> LeagueManagerActions { get; }
    public PublisherStateSet PublisherStateSet { get; }
    public IReadOnlyList<PublisherGame> AddedPublisherGames { get; }
    public IReadOnlyList<FormerPublisherGame> RemovedPublisherGames { get; }

    public IReadOnlyList<LeagueYearPublisherPair> GetAllAffectedPublisherPairs()
    {
        var publisherPairsToInclude = SuccessBids.Select(x => x.PickupBid.GetLeagueYearPublisherPair())
            .Concat(FailedBids.Select(x => x.PickupBid.GetLeagueYearPublisherPair()))
            .Concat(SuccessDrops.Select(x => x.GetLeagueYearPublisherPair()))
            .Concat(FailedDrops.Select(x => x.GetLeagueYearPublisherPair()));

        List<LeagueYearPublisherPair> updatedPublisherPairs = new List<LeagueYearPublisherPair>();
        foreach (var publisherPair in publisherPairsToInclude)
        {
            var updatedPublisher = PublisherStateSet.GetPublisher(publisherPair.Publisher.PublisherID);
            updatedPublisherPairs.Add(new LeagueYearPublisherPair(publisherPair.LeagueYear, updatedPublisher));
        }

        return updatedPublisherPairs;
    }

    public ActionProcessingResults MakeCopy()
    {
        return new ActionProcessingResults(SuccessBids, FailedBids, SuccessDrops, FailedDrops, LeagueActions,
            PublisherStateSet.MakeCopy(), AddedPublisherGames, RemovedPublisherGames, LeagueManagerActions);
    }

    public static ActionProcessingResults GetEmptyResultsSet(PublisherStateSet publisherStateSet)
    {
        return new ActionProcessingResults(new List<SucceededPickupBid>(), new List<FailedPickupBid>(), new List<DropRequest>(),
            new List<DropRequest>(), new List<LeagueAction>(), publisherStateSet, new List<PublisherGame>(), new List<FormerPublisherGame>(), new List<LeagueManagerAction>());
    }

    public static ActionProcessingResults GetResultsSetFromDropResults(IEnumerable<DropRequest> successDrops, IEnumerable<DropRequest> failedDrops,
        IEnumerable<LeagueAction> leagueActions, PublisherStateSet publisherStateSet, IEnumerable<FormerPublisherGame> droppedPublisherGames)
    {
        return new ActionProcessingResults(new List<SucceededPickupBid>(), new List<FailedPickupBid>(), successDrops,
            failedDrops, leagueActions, publisherStateSet, new List<PublisherGame>(), droppedPublisherGames, new List<LeagueManagerAction>());
    }

    public static ActionProcessingResults GetResultsSetFromBidResults(IEnumerable<SucceededPickupBid> successBids, IEnumerable<FailedPickupBid> simpleFailedBids,
        IEnumerable<LeagueAction> leagueActions, PublisherStateSet publisherStateSet, IEnumerable<PublisherGame> gamesToAdd, IEnumerable<FormerPublisherGame> droppedPublisherGames,
        IEnumerable<LeagueManagerAction> leagueManagerActions)
    {
        return new ActionProcessingResults(successBids, simpleFailedBids, new List<DropRequest>(),
            new List<DropRequest>(), leagueActions, publisherStateSet, gamesToAdd, droppedPublisherGames, leagueManagerActions);
    }

    public ActionProcessingResults Combine(ActionProcessingResults subProcessingResults)
    {
        return new ActionProcessingResults(
            SuccessBids.Concat(subProcessingResults.SuccessBids).DistinctBy(x => x.PickupBid.BidID),
            FailedBids.Concat(subProcessingResults.FailedBids).DistinctBy(x => x.PickupBid.BidID),
            SuccessDrops.Concat(subProcessingResults.SuccessDrops).DistinctBy(x => x.DropRequestID),
            FailedDrops.Concat(subProcessingResults.FailedDrops).DistinctBy(x => x.DropRequestID),
            LeagueActions.Concat(subProcessingResults.LeagueActions).DistinctBy(x => x.ActionInternalID),
            subProcessingResults.PublisherStateSet,
            AddedPublisherGames.Concat(subProcessingResults.AddedPublisherGames).DistinctBy(x => x.PublisherGameID),
            RemovedPublisherGames.Concat(subProcessingResults.RemovedPublisherGames).DistinctBy(x => x.PublisherGame.PublisherGameID),
            LeagueManagerActions.Concat(subProcessingResults.LeagueManagerActions));
    }
}
