using FantasyCritic.Lib.Domain.Requests;

namespace FantasyCritic.Lib.Extensions;

public static class PlayerGameExtensions
{
    public static bool ContainsGame(this IEnumerable<PublisherGame> games, MasterGame game)
    {
        bool containsGame = games.Any(x => x.MasterGame.HasValueTempoTemp && game.MasterGameID == x.MasterGame.ValueTempoTemp.MasterGame.MasterGameID);
        return containsGame;
    }

    public static bool ContainsGame(this IEnumerable<PublisherGame> games, PublisherGame game)
    {
        bool containsGame;
        if (game.MasterGame.HasValueTempoTemp)
        {
            containsGame = games.Any(x => x.MasterGame.HasValueTempoTemp && game.MasterGame.ValueTempoTemp.MasterGame.MasterGameID == x.MasterGame.ValueTempoTemp.MasterGame.MasterGameID);
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
        if (game.MasterGame.HasValueTempoTemp)
        {
            containsGame = games.Any(x => x.MasterGame.HasValueTempoTemp && game.MasterGame.ValueTempoTemp.MasterGameID == x.MasterGame.ValueTempoTemp.MasterGame.MasterGameID);
        }
        else
        {
            containsGame = games.Any(x => x.GameName == game.GameName);
        }

        return containsGame;
    }

    public static bool CounterPickedGameIsManualWillNotRelease(LeagueYear leagueYear, bool counterPick, Maybe<MasterGame> masterGame, bool gameCouldBeDropped)
    {
        if (!counterPick || masterGame.HasNoValueTempoTemp)
        {
            return false;
        }

        var gameBeingCounterPickedOptions = leagueYear.Publishers.Select(x => x.GetPublisherGame(masterGame.ValueTempoTemp))
            .Where(x => x.HasValueTempoTemp && !x.ValueTempoTemp.CounterPick).ToList();

        if (gameBeingCounterPickedOptions.Count != 1)
        {
            if (gameCouldBeDropped && gameBeingCounterPickedOptions.Count == 0)
            {
                return false;
            }

            throw new Exception($"Something very strange has happened with bid processing for league year: {leagueYear.Key}");
        }

        return gameBeingCounterPickedOptions.Single().ValueTempoTemp.ManualWillNotRelease;
    }
}
