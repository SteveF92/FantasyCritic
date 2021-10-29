using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain.LeagueActions
{
    public class DropProcessingResults
    {
        public DropProcessingResults(IEnumerable<DropRequest> successDrops, IEnumerable<DropRequest> failedDrops,
            IEnumerable<LeagueAction> leagueActions, IEnumerable<Publisher> publishersToUpdate, IEnumerable<PublisherGame> droppedPublisherGames)
        {
            SuccessDrops = successDrops.ToList();
            FailedDrops = failedDrops.ToList();
            LeagueActions = leagueActions.ToList();
            PublishersToUpdate = publishersToUpdate.ToList();
            DroppedPublisherGames = droppedPublisherGames.ToList();
        }

        public IReadOnlyList<DropRequest> SuccessDrops { get; }
        public IReadOnlyList<DropRequest> FailedDrops { get; }
        public IReadOnlyList<LeagueAction> LeagueActions { get; }
        public IReadOnlyList<Publisher> PublishersToUpdate { get; }
        public IReadOnlyList<PublisherGame> DroppedPublisherGames { get; }
    }
}
