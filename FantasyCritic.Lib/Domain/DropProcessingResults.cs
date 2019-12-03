using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class DropProcessingResults
    {
        public DropProcessingResults(IEnumerable<DropRequest> successDrops, IEnumerable<DropRequest> failedDrops, IEnumerable<LeagueAction> leagueActions,
            IEnumerable<Publisher> updatedPublishers, IEnumerable<PublisherGame> publisherGames)
        {
            SuccessDrops = successDrops;
            FailedDrops = failedDrops;
            LeagueActions = leagueActions;
            UpdatedPublishers = updatedPublishers;
            PublisherGames = publisherGames;
        }

        public IEnumerable<DropRequest> SuccessDrops { get; }
        public IEnumerable<DropRequest> FailedDrops { get; }
        public IEnumerable<LeagueAction> LeagueActions { get; }
        public IEnumerable<Publisher> UpdatedPublishers { get; }
        public IEnumerable<PublisherGame> PublisherGames { get; }

        public DropProcessingResults Combine(DropProcessingResults subProcessingResults)
        {
            return new DropProcessingResults(SuccessDrops.Concat(subProcessingResults.SuccessDrops),
                FailedDrops.Concat(subProcessingResults.FailedDrops),
                LeagueActions.Concat(subProcessingResults.LeagueActions),
                subProcessingResults.UpdatedPublishers,
                PublisherGames.Concat(subProcessingResults.PublisherGames));
        }
    }
}
