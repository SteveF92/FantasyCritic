namespace FantasyCritic.Lib.Domain;
public record MyGameNewsSet(IReadOnlyList<SingleGameNews> UpcomingGames, IReadOnlyList<SingleGameNews> RecentGames);
public record SingleGameNews(MasterGameYear MasterGameYear, IReadOnlyList<PublisherInfo> PublisherInfo);
public record PublisherInfo(Guid LeagueID, string LeagueName, int Year, Guid PublisherID, string PublisherName);
