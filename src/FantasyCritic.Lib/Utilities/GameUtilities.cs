namespace FantasyCritic.Lib.Utilities;

public static class GameUtilities
{
    public static IReadOnlyDictionary<PublisherGame, Publisher> GetCounterPickedByDictionary(LeagueYear leagueYear)
    {
        var gamesWithMasterGame = leagueYear.Publishers
            .SelectMany(x => x.PublisherGames)
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

    public static int GetTimesCounterPicked(LeagueYear leagueYear, Publisher publisher)
    {
        var allCounterPicksForOtherTeams = leagueYear.Publishers
                .Where(x => x.User.UserID != publisher.User.UserID)
                .SelectMany(x => x.PublisherGames)
                .Where(x => x.CounterPick)
                .ToList();

        int timesCounterPicked = 0;
        foreach (var counterPick in allCounterPicksForOtherTeams)
        {
            foreach (var myGame in publisher.PublisherGames)
            {
                if (myGame.CounterPick)
                {
                    continue;
                }

                if (myGame.Timestamp > counterPick.Timestamp)
                {
                    continue;
                }

                if (myGame.MasterGame is not null && counterPick.MasterGame is not null &&
                    myGame.MasterGame.MasterGame.MasterGameID == counterPick.MasterGame.MasterGame.MasterGameID)
                {
                    timesCounterPicked++;
                    break;
                }

                if (myGame.GameName == counterPick.GameName)
                {
                    timesCounterPicked++;
                    break;
                }
            }
        }

        return timesCounterPicked;
    }
}
