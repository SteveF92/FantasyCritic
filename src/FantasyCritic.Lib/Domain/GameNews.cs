namespace FantasyCritic.Lib.Domain;

public record MyGameNewsSet(IReadOnlyList<SingleGameNews> UpcomingGames, IReadOnlyList<SingleGameNews> RecentGames)
{
    public static MyGameNewsSet BuildMyGameNews(IReadOnlyList<SingleGameNews> myGameDetails, LocalDate currentDate)
    {
        var upcomingReleases = myGameDetails
            .Where(x => x.MasterGameYear.MasterGame.GetDefiniteMaximumReleaseDate() > currentDate)
            .OrderBy(x => x.MasterGameYear.MasterGame.GetDefiniteMaximumReleaseDate())
            .Take(10)
            .ToList();

        var recentReleases = myGameDetails
            .Where(x => x.MasterGameYear.MasterGame.GetDefiniteMaximumReleaseDate() <= currentDate)
            .OrderByDescending(x => x.MasterGameYear.MasterGame.GetDefiniteMaximumReleaseDate())
            .Take(10)
            .ToList();

        return new MyGameNewsSet(upcomingReleases, recentReleases);
    }
}
public record SingleGameNews(MasterGameYear MasterGameYear, IReadOnlyList<PublisherInfo> PublisherInfo);
public record PublisherInfo(Guid LeagueID, string LeagueName, int Year, Guid PublisherID, string PublisherName, bool CounterPick);
