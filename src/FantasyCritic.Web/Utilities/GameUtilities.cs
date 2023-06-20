namespace FantasyCritic.Web.Utilities;

public static class GameUtilities
{
    public static IReadOnlyDictionary<PublisherGame, Publisher> GetCounterPickedByDictionary(LeagueYear leagueYear)
    {
        var gamesWithMasterGame = leagueYear.Publishers.SelectMany(x => x.PublisherGames)
            .Where(x => x.MasterGame is not null)
            .ToList();
        var counterPicks = gamesWithMasterGame
            .Where(x => x.CounterPick)
            .ToDictionary(x => x.MasterGame!.MasterGame);

        Dictionary<PublisherGame, Publisher> dictionary = new Dictionary<PublisherGame, Publisher>();
        var nonCounterPicks = gamesWithMasterGame.Where(x => !x.CounterPick).ToList();

        foreach (var nonCounterPick in nonCounterPicks)
        {
            var counterPick = counterPicks.GetValueOrDefault(nonCounterPick.MasterGame!.MasterGame);
            if (counterPick is null)
            {
                continue;
            }

            var publisher = leagueYear.GetPublisherByID(counterPick.PublisherID);
            dictionary.Add(nonCounterPick, publisher!);
        }

        return dictionary;
    }
}
