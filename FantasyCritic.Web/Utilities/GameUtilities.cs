using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Utilities
{
    public static class GameUtilities
    {
        public static IReadOnlySet<Guid> GetDropBlockedPublisherGameIDs(LeagueYear leagueYear, IReadOnlyList<Publisher> publishers)
        {
            HashSet<Guid> dropBlockedPublisherGameIDs = new HashSet<Guid>();
            if (!leagueYear.Options.CounterPicksBlockDrops)
            {
                return dropBlockedPublisherGameIDs;
            }

            var gamesWithMasterGame = publishers.SelectMany(x => x.PublisherGames)
                .Where(x => x.MasterGame.HasValue)
                .ToList();
            var counterPicks = gamesWithMasterGame
                .Where(x => x.CounterPick)
                .Select(x => x.MasterGame.Value.MasterGame.MasterGameID)
                .ToHashSet();
            dropBlockedPublisherGameIDs = gamesWithMasterGame
                .Where(x => !x.CounterPick && counterPicks.Contains(x.MasterGame.Value.MasterGame.MasterGameID))
                .Select(x => x.PublisherGameID)
                .ToHashSet();

            return dropBlockedPublisherGameIDs;
        }
    }
}
