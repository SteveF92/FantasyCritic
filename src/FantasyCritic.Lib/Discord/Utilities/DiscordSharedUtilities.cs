using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Discord.Utilities;
public static class DiscordSharedUtilities
{
    public static string? BuildGameMessage(Publisher? standardPublisher, Publisher? counterPickPublisher, MasterGame masterGame, string baseAddress)
    {
        var gameUrl = new GameUrlBuilder(baseAddress, masterGame.MasterGameID).BuildUrl(masterGame.GameName);

        if (standardPublisher is not null)
        {
            return counterPickPublisher is not null
                ? $"> **{masterGame.EstimatedReleaseDate}** - {gameUrl}\n > {standardPublisher.GetPublisherAndUserDisplayName()}\n > Counter Picked By {counterPickPublisher.GetPublisherAndUserDisplayName()}"
                : $"> **{masterGame.EstimatedReleaseDate}** - {gameUrl}\n > {standardPublisher.GetPublisherAndUserDisplayName()}";
        }

        return counterPickPublisher is not null
            ? $"> **{masterGame.EstimatedReleaseDate}** - {gameUrl}\n > Counter Pick For {counterPickPublisher!.GetPublisherAndUserDisplayName()}"
            : null;
    }

    public static IList<string> RankLeaguePublishers(LeagueYear leagueYear,
        FantasyCriticUser? previousYearWinner, SystemWideValues systemWideValues,
        LocalDate dateToCheck)
    {
        var rankedPublishers = leagueYear.Publishers.OrderByDescending(p
            => p.GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options));

        var publisherLines =
            rankedPublishers
                .Select((publisher, index) =>
                {
                    var totalPoints = publisher
                        .GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options);

                    var projectedPoints = publisher
                        .GetProjectedFantasyPoints(leagueYear,
                            systemWideValues, dateToCheck);

                    return BuildPublisherLine(index + 1, publisher, totalPoints, projectedPoints, dateToCheck,
                        previousYearWinner);
                });
        return publisherLines.ToList();
    }

    private static string BuildPublisherLine(int rank, Publisher publisher, decimal totalPoints, decimal projectedPoints, LocalDate currentDate, FantasyCriticUser? previousYearWinner)
    {
        var totalGames = publisher.PublisherGames
            .Count(x => !x.CounterPick);
        var allWillRelease = publisher.PublisherGames
            .Where(x => !x.CounterPick)
            .Where(x => x.MasterGame is not null)
            .Count(x => x.CouldRelease());

        var gamesReleased = publisher.PublisherGames
            .Where(x => !x.CounterPick)
            .Where(x => x.MasterGame is not null)
            .Count(x => x.MasterGame!.MasterGame.IsReleased(currentDate));

        var publisherLine = $"**{rank}.** ";
        if (!string.IsNullOrEmpty(publisher.PublisherIcon))
        {
            publisherLine += $"{publisher.PublisherIcon} ";
        }

        var crownEmoji = "";
        if (previousYearWinner is not null && publisher.User.Id == previousYearWinner.Id)
        {
            crownEmoji = " 👑";
        }

        publisherLine += $"**{publisher.GetPublisherAndUserDisplayName()}**{crownEmoji}\n";
        publisherLine += $"> **{Math.Round(totalPoints, 1)} points** ";
        publisherLine += $"*(Projected: {Math.Round(projectedPoints, 1)})*\n";
        publisherLine += $"> {gamesReleased}/{allWillRelease} games released";
        var willNotRelease = totalGames - allWillRelease;
        if (willNotRelease != 0)
        {
            publisherLine += $" (and {willNotRelease} that will not release)";
        }

        return publisherLine;
    }
}
