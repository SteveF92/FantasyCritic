namespace FantasyCritic.Web.Utilities;

public static class GameUtilities
{
    public static IReadOnlySet<Guid> GetCounterPickedPublisherGameIDs(LeagueYear leagueYear)
    {
        var gamesWithMasterGame = leagueYear.Publishers.SelectMany(x => x.PublisherGames)
            .Where(x => x.MasterGame.HasValueTempoTemp)
            .ToList();
        var counterPicks = gamesWithMasterGame
            .Where(x => x.CounterPick)
            .Select(x => x.MasterGame.ValueTempoTemp.MasterGame.MasterGameID)
            .ToHashSet();
        HashSet<Guid> counterPickedPublisherGameIDs = gamesWithMasterGame
            .Where(x => !x.CounterPick && counterPicks.Contains(x.MasterGame.ValueTempoTemp.MasterGame.MasterGameID))
            .Select(x => x.PublisherGameID)
            .ToHashSet();

        return counterPickedPublisherGameIDs;
    }
}
