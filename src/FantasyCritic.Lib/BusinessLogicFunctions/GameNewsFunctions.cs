using FantasyCritic.Lib.Domain.Combinations;

namespace FantasyCritic.Lib.BusinessLogicFunctions;
public static class GameNewsFunctions
{
    public static IReadOnlyList<IGrouping<MasterGameYear, PublisherGame>> GetGameNews(IEnumerable<LeagueYearPublisherPair> publishers,
        LocalDate currentDate, bool recentReleases, int maxGamesCount)
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
                .Take(maxGamesCount);
        }
        else
        {
            orderedByReleaseDate = publisherGames
                .Distinct()
                .Where(x => x.MasterGame!.MasterGame.GetDefiniteMaximumReleaseDate() > yesterday)
                .OrderBy(x => x.MasterGame!.MasterGame.GetDefiniteMaximumReleaseDate())
                .GroupBy(x => x.MasterGame!)
                .Take(maxGamesCount);
        }

        return orderedByReleaseDate.ToList();
    }

    public static IReadOnlyDictionary<MasterGameYear, List<LeagueYearPublisherPair>> GetLeagueYearPublisherLists(IReadOnlyList<LeagueYearPublisherPair> publishers, IReadOnlyList<IGrouping<MasterGameYear, PublisherGame>> gameNews)
    {
        var leagueYearPublisherLists = new Dictionary<MasterGameYear, List<LeagueYearPublisherPair>>();

        foreach (var publisherGameGroup in gameNews)
        {
            var publishersThatHaveGame = publishers.Where(x => publisherGameGroup.Select(y => y.PublisherID).Contains(x.Publisher.PublisherID)).ToList();
            leagueYearPublisherLists.Add(publisherGameGroup.Key, publishersThatHaveGame);
        }

        return leagueYearPublisherLists;
    }
}
