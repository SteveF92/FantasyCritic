using FantasyCritic.Lib.Domain.Requests;

namespace FantasyCritic.Lib.Extensions;

public static class PlayerGameExtensions
{
    public static bool ContainsGame(this IEnumerable<PublisherGame> games, MasterGame game)
    {
        bool containsGame = games.Any(x => x.MasterGame is not null && game.MasterGameID == x.MasterGame.MasterGame.MasterGameID);
        return containsGame;
    }

    public static bool ContainsGame(this IEnumerable<PublisherGame> games, PublisherGame game)
    {
        bool containsGame;
        if (game.MasterGame is not null)
        {
            containsGame = games.Any(x => x.MasterGame is not null && game.MasterGame.MasterGame.MasterGameID == x.MasterGame.MasterGame.MasterGameID);
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
        if (game.MasterGame is not null)
        {
            containsGame = games.Any(x => x.MasterGame is not null && game.MasterGame.MasterGameID == x.MasterGame.MasterGame.MasterGameID);
        }
        else
        {
            containsGame = games.Any(x => x.GameName == game.GameName);
        }

        return containsGame;
    }

    public static bool CounterPickedGameIsManualWillNotRelease(LeagueYear leagueYear, bool counterPick, MasterGame? masterGame)
    {
        if (!counterPick || masterGame is null)
        {
            return false;
        }

        var gameBeingCounterPickedOptions = leagueYear.Publishers.Select(x => x.GetPublisherGame(masterGame, false))
            .Where(x => x is not null && !x.CounterPick).Select(x => x!).ToList();

        // Nobody publishing the game is a normal claim error, which eligibility reports; two publishers holding it is corrupt data.
        if (gameBeingCounterPickedOptions.Count == 0)
        {
            return false;
        }

        if (gameBeingCounterPickedOptions.Count > 1)
        {
            throw new Exception($"Multiple publishers hold {masterGame.GameName} as a standard game in league year: {leagueYear.Key}");
        }

        return gameBeingCounterPickedOptions.Single().ManualWillNotRelease;
    }
}
