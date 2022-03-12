namespace FantasyCritic.Lib.Domain.LeagueActions
{
    public class ActionProcessingResults
    {
        private ActionProcessingResults(IEnumerable<SucceededPickupBid> successBids, IEnumerable<FailedPickupBid> failedBids,
            IEnumerable<DropRequest> successDrops, IEnumerable<DropRequest> failedDrops, IEnumerable<LeagueAction> leagueActions,
            IEnumerable<Publisher> updatedPublishers, IEnumerable<PublisherGame> addedPublisherGames, IEnumerable<FormerPublisherGame> removedPublisherGames)
        {
            SuccessBids = successBids.ToList();
            FailedBids = failedBids.ToList();
            SuccessDrops = successDrops.ToList();
            FailedDrops = failedDrops.ToList();
            LeagueActions = leagueActions.ToList();
            UpdatedPublishers = updatedPublishers.ToList();
            AddedPublisherGames = addedPublisherGames.ToList();
            RemovedPublisherGames = removedPublisherGames.ToList();
        }

        public IReadOnlyList<SucceededPickupBid> SuccessBids { get; }
        public IReadOnlyList<FailedPickupBid> FailedBids { get; }
        public IReadOnlyList<DropRequest> SuccessDrops { get; }
        public IReadOnlyList<DropRequest> FailedDrops { get; }
        public IReadOnlyList<LeagueAction> LeagueActions { get; }
        public IReadOnlyList<Publisher> UpdatedPublishers { get; }
        public IReadOnlyList<PublisherGame> AddedPublisherGames { get; }
        public IReadOnlyList<FormerPublisherGame> RemovedPublisherGames { get; }

        public static ActionProcessingResults GetEmptyResultsSet(IEnumerable<Publisher> publisherStates)
        {
            return new ActionProcessingResults(new List<SucceededPickupBid>(), new List<FailedPickupBid>(), new List<DropRequest>(),
                new List<DropRequest>(), new List<LeagueAction>(), publisherStates, new List<PublisherGame>(), new List<FormerPublisherGame>());
        }

        public static ActionProcessingResults GetResultsSetFromDropResults(IEnumerable<DropRequest> successDrops, IEnumerable<DropRequest> failedDrops,
            IEnumerable<LeagueAction> leagueActions, IEnumerable<Publisher> updatedPublishers, IEnumerable<FormerPublisherGame> droppedPublisherGames)
        {
            return new ActionProcessingResults(new List<SucceededPickupBid>(), new List<FailedPickupBid>(), successDrops,
                failedDrops, leagueActions, updatedPublishers, new List<PublisherGame>(), droppedPublisherGames);
        }

        public static ActionProcessingResults GetResultsSetFromBidResults(IEnumerable<SucceededPickupBid> successBids, IEnumerable<FailedPickupBid> simpleFailedBids,
            IEnumerable<LeagueAction> leagueActions, IEnumerable<Publisher> updatedPublishers, IEnumerable<PublisherGame> gamesToAdd, IEnumerable<FormerPublisherGame> droppedPublisherGames)
        {
            return new ActionProcessingResults(successBids, simpleFailedBids, new List<DropRequest>(),
                new List<DropRequest>(), leagueActions, updatedPublishers, gamesToAdd, droppedPublisherGames);
        }

        public ActionProcessingResults Combine(ActionProcessingResults subProcessingResults)
        {
            return new ActionProcessingResults(
                SuccessBids.Concat(subProcessingResults.SuccessBids).DistinctBy(x => x.PickupBid.BidID),
                FailedBids.Concat(subProcessingResults.FailedBids).DistinctBy(x => x.PickupBid.BidID),
                SuccessDrops.Concat(subProcessingResults.SuccessDrops).DistinctBy(x => x.DropRequestID),
                FailedDrops.Concat(subProcessingResults.FailedDrops).DistinctBy(x => x.DropRequestID),
                LeagueActions.Concat(subProcessingResults.LeagueActions).DistinctBy(x => x.ActionInternalID),
                subProcessingResults.UpdatedPublishers,
                AddedPublisherGames.Concat(subProcessingResults.AddedPublisherGames).DistinctBy(x => x.PublisherGameID),
                RemovedPublisherGames.Concat(subProcessingResults.RemovedPublisherGames).DistinctBy(x => x.PublisherGame.PublisherGameID));
        }
    }
}
