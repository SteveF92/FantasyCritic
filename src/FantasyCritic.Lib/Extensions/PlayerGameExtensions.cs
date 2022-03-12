using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
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

        public static bool CounterPickedGameIsManualWillNotRelease(LeagueYear leagueYear, 
            IEnumerable<Publisher> publishersInLeagueYear, bool counterPick, Maybe<MasterGame> masterGame, bool gameCouldBeDropped)
        {
            if (!counterPick || masterGame.HasNoValue)
            {
                return false;
            }

            var gameBeingCounterPickedOptions = publishersInLeagueYear.Select(x => x.GetPublisherGame(masterGame.Value))
                    .Where(x => x.HasValue && !x.Value.CounterPick).ToList();

            if (gameBeingCounterPickedOptions.Count != 1)
            {
                if (gameCouldBeDropped && gameBeingCounterPickedOptions.Count == 0)
                {
                    return false;
                }

                throw new Exception($"Something very strange has happened with bid processing for league year: {leagueYear.Key}");
            }

            return gameBeingCounterPickedOptions.Single().Value.ManualWillNotRelease;
        }
    }
}
