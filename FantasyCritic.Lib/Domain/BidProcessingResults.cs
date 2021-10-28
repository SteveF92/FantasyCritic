using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class ActionProcessingResults
    {
        public ActionProcessingResults(IEnumerable<PickupBid> successBids, IEnumerable<PickupBid> failedBids, IEnumerable<LeagueAction> leagueActions,
            IEnumerable<Publisher> updatedPublishers, IEnumerable<PublisherGame> publisherGames)
        {
            SuccessBids = successBids;
            FailedBids = failedBids;
            LeagueActions = leagueActions;
            UpdatedPublishers = updatedPublishers;
            PublisherGames = publisherGames;
        }

        public IEnumerable<PickupBid> SuccessBids { get; }
        public IEnumerable<PickupBid> FailedBids { get; }
        public IEnumerable<LeagueAction> LeagueActions { get; }
        public IEnumerable<Publisher> UpdatedPublishers { get; }
        public IEnumerable<PublisherGame> PublisherGames { get; }

        public ActionProcessingResults Combine(ActionProcessingResults subProcessingResults)
        {
            return new ActionProcessingResults(SuccessBids.Concat(subProcessingResults.SuccessBids),
                FailedBids.Concat(subProcessingResults.FailedBids),
                LeagueActions.Concat(subProcessingResults.LeagueActions),
                subProcessingResults.UpdatedPublishers,
                PublisherGames.Concat(subProcessingResults.PublisherGames));
        }
    }
}
