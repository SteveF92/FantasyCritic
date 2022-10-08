namespace FantasyCritic.Lib.BusinessLogicFunctions;
public static class GameNewsFunctions
{
    public static IReadOnlyList<IGrouping<MasterGameYear, PublisherGame>> GetGameNews(IEnumerable<LeagueYearPublisherPair> publishers, bool recentReleases, LocalDate currentDate)
    {
        var publisherGames = publishers.SelectMany(x => x.Publisher.PublisherGames).Where(x => x.MasterGame is not null);
        var yesterday = currentDate.PlusDays(-1);
        var tomorrow = currentDate.PlusDays(1);

        IEnumerable<IGrouping<MasterGameYear, PublisherGame>> orderedByReleaseDate;

        if (recentReleases)
        {
            orderedByReleaseDate = publisherGames
                .Distinct()
                .Where(x => x.MasterGame!.MasterGame.GetDefiniteMaximumReleaseDate() < tomorrow)
                .OrderByDescending(x => x.MasterGame!.MasterGame.GetDefiniteMaximumReleaseDate())
                .GroupBy(x => x.MasterGame!)
                .Take(10);
        }
        else
        {
            orderedByReleaseDate = publisherGames
                .Distinct()
                .Where(x => x.MasterGame!.MasterGame.GetDefiniteMaximumReleaseDate() > yesterday)
                .OrderBy(x => x.MasterGame!.MasterGame.GetDefiniteMaximumReleaseDate())
                .GroupBy(x => x.MasterGame!)
                .Take(10);
        }

        return orderedByReleaseDate.ToList();
    }
}
