using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Lib.Extensions
{
    public static class PlayerGameExtensions
    {
        public static bool ContainsGame(this IEnumerable<PlayerGame> games, PlayerGame game)
        {
            bool containsGame;
            if (game.MasterGame.HasValue)
            {
                containsGame = games.Any(x => x.MasterGame.HasValue && game.MasterGame.Value.MasterGameID == x.MasterGame.Value.MasterGameID);
            }
            else
            {
                containsGame = games.Any(x => x.GameName == game.GameName);
            }

            return containsGame;
        }

        public static bool ContainsGame(this IEnumerable<PlayerGame> games, ClaimGameDomainRequest game)
        {
            bool containsGame;
            if (game.MasterGame.HasValue)
            {
                containsGame = games.Any(x => x.MasterGame.HasValue && game.MasterGame.Value.MasterGameID == x.MasterGame.Value.MasterGameID);
            }
            else
            {
                containsGame = games.Any(x => x.GameName == game.GameName);
            }

            return containsGame;
        }
    }
}
