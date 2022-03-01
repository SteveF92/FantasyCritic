using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain.LeagueActions;
using NodaTime;

namespace FantasyCritic.Lib.Domain.Trades
{
    public class ExecutedTrade
    {
        public ExecutedTrade(Trade trade, Instant completionTime, IEnumerable<PublisherGame> addedPublisherGames)
        {
            Trade = trade;
            CompletionTime = completionTime;
            LeagueActions = trade.GetActions(completionTime);
            UpdatedPublishers = trade.GetUpdatedPublishers();
            AddedPublisherGames = addedPublisherGames.ToList();
            RemovedPublisherGames = trade.GetRemovedPublisherGames(completionTime);
        }

        public Trade Trade { get; }
        public Instant CompletionTime { get; }
        public IReadOnlyList<LeagueAction> LeagueActions { get; }
        public IReadOnlyList<Publisher> UpdatedPublishers { get; }
        public IReadOnlyList<PublisherGame> AddedPublisherGames { get; }
        public IReadOnlyList<FormerPublisherGame> RemovedPublisherGames { get; }

    }
}
