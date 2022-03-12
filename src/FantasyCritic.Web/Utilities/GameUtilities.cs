namespace FantasyCritic.Web.Utilities;

public static class GameUtilities
{
    public static IReadOnlySet<Guid> GetCounterPickedPublisherGameIDs(LeagueYear leagueYear, IReadOnlyList<Publisher> publishers)
    {
        var gamesWithMasterGame = publishers.SelectMany(x => x.PublisherGames)
            .Where(x => x.MasterGame.HasValue)
            .ToList();
        var counterPicks = gamesWithMasterGame
            .Where(x => x.CounterPick)
            .Select(x => x.MasterGame.Value.MasterGame.MasterGameID)
            .ToHashSet();
        HashSet<Guid> counterPickedPublisherGameIDs = gamesWithMasterGame
            .Where(x => !x.CounterPick && counterPicks.Contains(x.MasterGame.Value.MasterGame.MasterGameID))
            .Select(x => x.PublisherGameID)
            .ToHashSet();

        return counterPickedPublisherGameIDs;
    }
}