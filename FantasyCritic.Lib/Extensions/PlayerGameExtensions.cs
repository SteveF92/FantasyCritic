using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;

namespace FantasyCritic.Lib.Extensions
{
    public static class PlayerGameExtensions
    {
        public static bool ContainsGame(this IEnumerable<PublisherGame> games, MasterGame game)
        {
            bool containsGame = games.Any(x => x.MasterGame.HasValue && game.MasterGameID == x.MasterGame.Value.MasterGame.MasterGameID);
            return containsGame;
        }

        public static bool ContainsGame(this IEnumerable<PublisherGame> games, PublisherGame game)
        {
            bool containsGame;
            if (game.MasterGame.HasValue)
            {
                containsGame = games.Any(x => x.MasterGame.HasValue && game.MasterGame.Value.MasterGame.MasterGameID == x.MasterGame.Value.MasterGame.MasterGameID);
            }
            else
            {
                containsGame = games.Any(x => x.GameName == game.GameName);
            }

            return containsGame;
        }

        public static bool ContainsGame(this IEnumerable<PublisherGame> games, ClaimGameDomainRequest game)
        {
            bool containsGame;
            if (game.MasterGame.HasValue)
            {
                containsGame = games.Any(x => x.MasterGame.HasValue && game.MasterGame.Value.MasterGameID == x.MasterGame.Value.MasterGame.MasterGameID);
            }
            else
            {
                containsGame = games.Any(x => x.GameName == game.GameName);
            }

            return containsGame;
        }
    }
}
