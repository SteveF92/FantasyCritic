using MoreLinq.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace FantasyCritic.Lib.Domain.LeagueActions
{
    public class ActionProcessingResults
    {
        private ActionProcessingResults(IEnumerable<PickupBid> successBids, IEnumerable<PickupBid> failedBids,
            IEnumerable<DropRequest> successDrops, IEnumerable<DropRequest> failedDrops, IEnumerable<LeagueAction> leagueActions,
            IEnumerable<Publisher> updatedPublishers, IEnumerable<PublisherGame> addedPublisherGames, IEnumerable<PublisherGame> removedPublisherGames)
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

        public IReadOnlyList<PickupBid> SuccessBids { get; }
        public IReadOnlyList<PickupBid> FailedBids { get; }
        public IReadOnlyList<DropRequest> SuccessDrops { get; }
        public IReadOnlyList<DropRequest> FailedDrops { get; }
        public IReadOnlyList<LeagueAction> LeagueActions { get; }
        public IReadOnlyList<Publisher> UpdatedPublishers { get; }
        public IReadOnlyList<PublisherGame> AddedPublisherGames { get; }
        public IReadOnlyList<PublisherGame> RemovedPublisherGames { get; }

        public static ActionProcessingResults GetEmptyResultsSet(IEnumerable<Publisher> publisherStates)
        {
            return new ActionProcessingResults(new List<PickupBid>(), new List<PickupBid>(), new List<DropRequest>(),
                new List<DropRequest>(), new List<LeagueAction>(), publisherStates, new List<PublisherGame>(), new List<PublisherGame>());
        }

        public static ActionProcessingResults GetResultsSetFromDropResults(IEnumerable<DropRequest> successDrops, IEnumerable<DropRequest> failedDrops,
            IEnumerable<LeagueAction> leagueActions, IEnumerable<Publisher> updatedPublishers, IEnumerable<PublisherGame> droppedPublisherGames)
        {
            return new ActionProcessingResults(new List<PickupBid>(), new List<PickupBid>(), successDrops,
                failedDrops, leagueActions, updatedPublishers, new List<PublisherGame>(), droppedPublisherGames);
        }

        public static ActionProcessingResults GetResultsSetFromBidResults(IEnumerable<PickupBid> successBids, IEnumerable<PickupBid> simpleFailedBids,
            IEnumerable<LeagueAction> leagueActions, IEnumerable<Publisher> updatedPublishers, IEnumerable<PublisherGame> gamesToAdd, IEnumerable<PublisherGame> droppedPublisherGames)
        {
            return new ActionProcessingResults(successBids, simpleFailedBids, new List<DropRequest>(),
                new List<DropRequest>(), leagueActions, updatedPublishers, gamesToAdd, droppedPublisherGames);
        }

        public ActionProcessingResults Combine(ActionProcessingResults subProcessingResults)
        {
            return new ActionProcessingResults(
                SuccessBids.Concat(subProcessingResults.SuccessBids).DistinctBy(x => x.BidID),
                FailedBids.Concat(subProcessingResults.FailedBids).DistinctBy(x => x.BidID), 
                SuccessDrops.Concat(subProcessingResults.SuccessDrops).DistinctBy(x => x.DropRequestID), 
                FailedDrops.Concat(subProcessingResults.FailedDrops).DistinctBy(x => x.DropRequestID), 
                LeagueActions.Concat(subProcessingResults.LeagueActions).DistinctBy(x => x.ActionInternalID),
                subProcessingResults.UpdatedPublishers, 
                AddedPublisherGames.Concat(subProcessingResults.AddedPublisherGames).DistinctBy(x => x.PublisherGameID), 
                RemovedPublisherGames.Concat(subProcessingResults.RemovedPublisherGames).DistinctBy(x => x.PublisherGameID));
        }
    }
}
