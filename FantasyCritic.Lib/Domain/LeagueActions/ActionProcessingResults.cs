using System.Collections.Generic;
using System.Linq;

namespace FantasyCritic.Lib.Domain.LeagueActions
{
    public class ActionProcessingResults
    {
        public ActionProcessingResults(IEnumerable<PickupBid> successBids, IEnumerable<PickupBid> failedBids,
            IEnumerable<DropRequest> successDrops, IEnumerable<DropRequest> failedDrops, IEnumerable<LeagueAction> leagueActions,
            IEnumerable<PublisherActionStatus> updatedPublishers, IEnumerable<PublisherGame> addedPublisherGames, IEnumerable<PublisherGame> removedPublisherGames)
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
        public IReadOnlyList<PublisherActionStatus> UpdatedPublishers { get; }
        public IReadOnlyList<PublisherGame> AddedPublisherGames { get; }
        public IReadOnlyList<PublisherGame> RemovedPublisherGames { get; }

        public static ActionProcessingResults GetEmptyResultsSet(IEnumerable<PublisherActionStatus> publisherStates)
        {
            return new ActionProcessingResults(new List<PickupBid>(), new List<PickupBid>(), new List<DropRequest>(),
                new List<DropRequest>(), new List<LeagueAction>(), publisherStates, new List<PublisherGame>(), new List<PublisherGame>());
        }
    }
}
