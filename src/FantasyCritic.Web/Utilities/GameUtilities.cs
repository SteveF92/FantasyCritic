namespace FantasyCritic.Web.Utilities;

public static class GameUtilities
{
    public static IReadOnlySet<Guid> GetCounterPickedPublisherGameIDs(LeagueYear leagueYear)
    {
        var gamesWithMasterGame = leagueYear.Publishers.SelectMany(x => x.PublisherGames)
            .Where(x => x.MasterGame is not null)
            .ToList();
        var counterPicks = gamesWithMasterGame
            .Where(x => x.CounterPick)
            .Select(x => x.MasterGame.MasterGame.MasterGameID)
            .ToHashSet();
        HashSet<Guid> counterPickedPublisherGameIDs = gamesWithMasterGame
            .Where(x => !x.CounterPick && counterPicks.Contains(x.MasterGame.MasterGame.MasterGameID))
            .Select(x => x.PublisherGameID)
            .ToHashSet();

        return counterPickedPublisherGameIDs;
    }
}
