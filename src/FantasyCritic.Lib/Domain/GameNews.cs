namespace FantasyCritic.Lib.Domain;

public record MyGameNewsSet(IReadOnlyList<SingleGameNews> UpcomingGames, IReadOnlyList<SingleGameNews> RecentGames)
{
    public static MyGameNewsSet BuildMyGameNews(IReadOnlyList<SingleGameNews> myGameDetails, LocalDate currentDate)
    {
        var upcomingReleases = myGameDetails
            .Where(x => x.MasterGameYear.MasterGame.GetDefiniteMaximumReleaseDate() > currentDate)
            .Take(10)
            .OrderBy(x => x.MasterGameYear.MasterGame.GetDefiniteMaximumReleaseDate())
            .ToList();

        var recentReleases = myGameDetails
            .Where(x => x.MasterGameYear.MasterGame.GetDefiniteMaximumReleaseDate() <= currentDate)
            .Take(10)
            .OrderByDescending(x => x.MasterGameYear.MasterGame.GetDefiniteMaximumReleaseDate())
            .ToList();

        return new MyGameNewsSet(upcomingReleases, recentReleases);
    }
}
public record SingleGameNews(MasterGameYear MasterGameYear, IReadOnlyList<PublisherInfo> PublisherInfo);
public record PublisherInfo(Guid LeagueID, string LeagueName, int Year, Guid PublisherID, string PublisherName, bool CounterPick);
