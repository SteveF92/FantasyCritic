using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Discord.Utilities;
public static class DiscordSharedMessageUtilities
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

    public static string BuildGameWithPublishersMessage(SingleGameNews gameNews, string baseAddress)
    {
        var masterGameYear = gameNews.MasterGameYear;
        var gameUrl = new GameUrlBuilder(baseAddress, masterGameYear.MasterGame.MasterGameID).BuildUrl(masterGameYear.MasterGame.GameName);

        var joinedPublisherLeagueNames = string.Join("\n> ",
            gameNews.PublisherInfo.Select(p =>
            {
                var leagueUrl = new LeagueUrlBuilder(baseAddress, p.LeagueID, p.Year).BuildUrl(p.LeagueName);
                return $"{leagueUrl} {(p.CounterPick ? "(Counter Pick)" : "")}";
            }));
        return $"**{masterGameYear.MasterGame.EstimatedReleaseDate}** - {gameUrl}\n> {joinedPublisherLeagueNames}";
    }

    public static IList<string> RankLeaguePublishers(LeagueYear leagueYear,
        FantasyCriticUser? previousYearWinner, SystemWideValues systemWideValues,
        LocalDate dateToCheck, bool isFinal = false)
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
                            systemWideValues);

                    return BuildPublisherLine(index + 1, publisher, totalPoints, projectedPoints, dateToCheck,
                        previousYearWinner, isFinal);
                });
        return publisherLines.ToList();
    }

    public static IList<string> RankConferencePublishers(IReadOnlyList<ConferenceYearStanding> conferenceYearStandings, bool isFinal = false)
    {
        var conferencePublisherStandings = conferenceYearStandings.Select((c, index) =>
            BuildConferencePublisherLine(index + 1, c.PublisherName, c.DisplayName, c.TotalFantasyPoints, c.ProjectedFantasyPoints, isFinal));
        return conferencePublisherStandings.ToList();
    }

    private static string BuildConferencePublisherLine(int rank, string publisherName, string displayName,
        decimal totalFantasyPoints, decimal projectedFantasyPoints, bool isFinal)
    {
        var shouldHighlight = ShouldPublisherBeHighlighted(rank, isFinal);

        var conferencePublisherLine = $"**{rank}.** ";

        var trophyEmoji = "";
        if (isFinal && rank == 1)
        {
            trophyEmoji = " 🏆";
        }

        conferencePublisherLine += shouldHighlight
            ? $"__**{publisherName} ({displayName})**__{trophyEmoji}\n"
            : $"**{publisherName} ({displayName})**{trophyEmoji}\n";
        conferencePublisherLine += $"> **{Math.Round(totalFantasyPoints, 1)} points**";
        if (!isFinal)
        {
            conferencePublisherLine += $" *(Projected: {Math.Round(projectedFantasyPoints, 1)})*\n";
        }
        return conferencePublisherLine;
    }

    private static string BuildPublisherLine(int rank,
        Publisher publisher,
        decimal totalPoints,
        decimal projectedPoints,
        LocalDate currentDate,
        FantasyCriticUser? previousYearWinner,
        bool isFinal)
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

        var trophyEmoji = "";
        if (isFinal && rank == 1)
        {
            trophyEmoji = " 🏆";
        }

        var shouldHighlight = ShouldPublisherBeHighlighted(rank, isFinal);

        publisherLine += shouldHighlight
            ? $"__**{publisher.GetPublisherAndUserDisplayName()}**__{crownEmoji}{trophyEmoji}\n"
            : $"**{publisher.GetPublisherAndUserDisplayName()}**{crownEmoji}{trophyEmoji}\n";
        publisherLine += $"> **{Math.Round(totalPoints, 1)} points** ";

        if (!isFinal)
        {
            publisherLine += $"*(Projected: {Math.Round(projectedPoints, 1)})*\n";

            publisherLine += $"> {gamesReleased}/{allWillRelease} games released";
            var willNotRelease = totalGames - allWillRelease;
            if (willNotRelease != 0)
            {
                publisherLine += $" (and {willNotRelease} that will not release)";
            }
        }
        else
        {
            publisherLine += "\n";
            publisherLine += $"> Released {gamesReleased} games this year\n";
        }

        return publisherLine;
    }

    private static bool ShouldPublisherBeHighlighted(int rank, bool highlightCurrentWinner)
    {
        if (!highlightCurrentWinner)
        {
            return false;
        }
        return rank == 1;
    }

    public static string BuildBidResultMessage(PickupBid bid)
    {
        var message = "";
        var counterPickMessage = bid.CounterPick ? "(🎯 Counter Pick)" : "";
        if (bid.Successful!.Value)
        {
            message += $"- Won by {bid.Publisher.GetPublisherAndUserDisplayName()} with a bid of ${bid.BidAmount} {counterPickMessage}";
            if (bid.ConditionalDropPublisherGame is null)
            {
                return message;
            }

            if (!bid.Outcome!.Contains("Attempted to conditionally drop"))
            {
                message += $"\n\t Dropped game '{bid.ConditionalDropPublisherGame.GameName}' conditionally";
            }
            else
            {
                int startIndex = bid.Outcome.IndexOf("Attempted to conditionally drop", StringComparison.Ordinal);
                string conditionalDropOutcome = bid.Outcome[startIndex..].TrimEnd('.');
                message += $"\n\t {conditionalDropOutcome}";
            }
        }
        else
        {
            message += $"- {bid.Publisher.GetPublisherAndUserDisplayName()}'s bid of ${bid.BidAmount} did not succeed: {bid.Outcome} {counterPickMessage}";
        }
        return message;
    }

    public static string BuildDropResultMessage(DropRequest drop)
    {
        if (!drop.Successful.HasValue)
        {
            throw new Exception($"Drop {drop.DropRequestID} Successful property is null");
        }

        var statusMessage = drop.Successful.Value ? "Successful" : "Failed";
        return $"**{drop.Publisher.GetPublisherAndUserDisplayName()}**: {drop.MasterGame.GameName} (Drop {statusMessage})";
    }

    public static string BuildPublicBidGameMessage(PublicBiddingMasterGame publicBid)
    {
        var gameMessage = "";
        var releaseDate = publicBid.MasterGameYear.MasterGame.EstimatedReleaseDate;
        gameMessage += $"**{publicBid.MasterGameYear.MasterGame.GameName}**";

        if (publicBid.CounterPick)
        {
            gameMessage += " (🎯 Counter Pick Bid)";
        }

        gameMessage += $"\n> Release Date: {releaseDate}";

        var roundedHypeFactor = Math.Round(publicBid.MasterGameYear.HypeFactor, 1);
        gameMessage += $"\n> Hype Factor: {roundedHypeFactor}\n";
        return gameMessage;
    }

    public static string BuildRemainingTimeMessage(Duration duration)
    {
        return $"{(duration.Days > 0 ? $"{duration.Days} days, " : "")}{(duration.Hours > 0 ? $"{duration.Hours} hours, " : "")}{duration.Minutes} minutes";
    }
}
