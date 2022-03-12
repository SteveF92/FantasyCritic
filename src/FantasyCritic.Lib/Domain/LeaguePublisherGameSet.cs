using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class LeaguePublisherGameSet
    {
        public LeaguePublisherGameSet(Guid playerPublisherID, IEnumerable<Publisher> allPublishersInLeague)
        {
            var thisPlayerStandardGames = new List<PublisherGame>();
            var thisPlayerCounterPicks = new List<PublisherGame>();
            var otherPlayerStandardGames = new List<PublisherGame>();
            var otherPlayerCounterPicks = new List<PublisherGame>();
            foreach (var publisher in allPublishersInLeague)
            {
                foreach (var publisherGame in publisher.PublisherGames)
                {
                    if (publisher.PublisherID == playerPublisherID)
                    {
                        if (!publisherGame.CounterPick)
                        {
                            thisPlayerStandardGames.Add(publisherGame);
                        }
                        else
                        {
                            thisPlayerCounterPicks.Add(publisherGame);
                        }
                    }
                    else
                    {
                        if (!publisherGame.CounterPick)
                        {
                            otherPlayerStandardGames.Add(publisherGame);
                        }
                        else
                        {
                            otherPlayerCounterPicks.Add(publisherGame);
                        }
                    }
                }
            }

            ThisPlayerStandardGames = thisPlayerStandardGames;
            ThisPlayerCounterPicks = thisPlayerCounterPicks;
            OtherPlayerStandardGames = otherPlayerStandardGames;
            OtherPlayerCounterPicks = otherPlayerCounterPicks;
        }

        public IReadOnlyList<PublisherGame> ThisPlayerStandardGames { get; }
        public IReadOnlyList<PublisherGame> ThisPlayerCounterPicks { get; }
        public IReadOnlyList<PublisherGame> OtherPlayerStandardGames { get; }
        public IReadOnlyList<PublisherGame> OtherPlayerCounterPicks { get; }
    }
}
