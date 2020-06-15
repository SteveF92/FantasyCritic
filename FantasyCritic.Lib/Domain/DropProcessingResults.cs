using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class DropProcessingResults
    {
        public DropProcessingResults(IEnumerable<DropRequest> successDrops, IEnumerable<DropRequest> failedDrops, 
            IEnumerable<LeagueAction> leagueActions, IEnumerable<Publisher> publishersToUpdate, IEnumerable<PublisherGame> publisherGames)
        {
            SuccessDrops = successDrops;
            FailedDrops = failedDrops;
            LeagueActions = leagueActions;
            PublishersToUpdate = publishersToUpdate;
            PublisherGames = publisherGames;
        }

        public IEnumerable<DropRequest> SuccessDrops { get; }
        public IEnumerable<DropRequest> FailedDrops { get; }
        public IEnumerable<LeagueAction> LeagueActions { get; }
        public IEnumerable<Publisher> PublishersToUpdate { get; }
        public IEnumerable<PublisherGame> PublisherGames { get; }
    }
}
