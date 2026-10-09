using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.Discord.Utilities;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Test.Draft;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class LeagueYearWinnerTests
{
    private const string Trophy = "🏆";
    private static readonly SystemWideValues SystemWideValues = new SystemWideValues(0m, 0m, 0m, [], []);
    private static readonly LocalDate EndOfYear = new LocalDate(2025, 12, 31);

    [Test]
    public void GetWinningPublisher_PublisherWithNoUserHasMostPoints_ReturnsTopPublisherWithUser()
    {
        var leagueYear = BuildLeagueYear(publisherCount: 3, disconnectedDraftPosition: 1, (1, 50m), (2, 30m), (3, 20m));

        var winner = leagueYear.GetWinningPublisher();

        Assert.That(winner?.PublisherName, Is.EqualTo("Publisher 2"));
    }

    [Test]
    public void GetWinningPublisher_Tie_ReturnsLaterDraftPosition()
    {
        var leagueYear = BuildLeagueYear(publisherCount: 3, disconnectedDraftPosition: null, (1, 30m), (2, 30m), (3, 20m));

        var winner = leagueYear.GetWinningPublisher();

        Assert.That(winner?.PublisherName, Is.EqualTo("Publisher 2"));
    }

    [Test]
    public void GetWinningPublisher_AllPointsNegative_ReturnsTopPublisher()
    {
        var leagueYear = BuildLeagueYear(publisherCount: 2, disconnectedDraftPosition: null, (1, -5m), (2, -10m));

        var winner = leagueYear.GetWinningPublisher();

        Assert.That(winner?.PublisherName, Is.EqualTo("Publisher 1"));
    }

    [Test]
    public void RankLeaguePublishers_Final_PublisherWithNoUserFirst_TrophyGoesToTopPublisherWithUser()
    {
        var leagueYear = BuildLeagueYear(publisherCount: 3, disconnectedDraftPosition: 1, (1, 50m), (2, 30m), (3, 20m));

        var lines = DiscordSharedMessageUtilities.RankLeaguePublishers(leagueYear, null, SystemWideValues, EndOfYear, isFinal: true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(lines[0], Does.StartWith("**1.** **Publisher 1"));
            Assert.That(lines[0], Does.Not.Contain(Trophy));
            Assert.That(lines[1], Does.StartWith("**2.** __**Publisher 2"));
            Assert.That(lines[1], Does.Contain(Trophy));
            Assert.That(lines.Count(x => x.Contains(Trophy)), Is.EqualTo(1));
        }
    }

    [Test]
    public void RankLeaguePublishers_NotFinal_NoTrophy()
    {
        var leagueYear = BuildLeagueYear(publisherCount: 3, disconnectedDraftPosition: null, (1, 50m), (2, 30m), (3, 20m));

        var lines = DiscordSharedMessageUtilities.RankLeaguePublishers(leagueYear, null, SystemWideValues, EndOfYear);

        Assert.That(lines.Any(x => x.Contains(Trophy) || x.Contains("__")), Is.False);
    }

    [Test]
    public void RankConferencePublishers_Final_PublisherWithNoUserFirst_TrophyGoesToTopPublisherWithUser()
    {
        var leagueID = Guid.NewGuid();
        IReadOnlyList<ConferenceYearStanding> standings =
        [
            new ConferenceYearStanding(leagueID, "League", 2025, Guid.NewGuid(), "<Non-Existent User>", "Disconnected Publisher", false, 50m, 50m),
            new ConferenceYearStanding(leagueID, "League", 2025, Guid.NewGuid(), "Player", "Winning Publisher", true, 30m, 30m)
        ];

        var lines = DiscordSharedMessageUtilities.RankConferencePublishers(standings, isFinal: true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(lines[0], Does.Not.Contain(Trophy));
            Assert.That(lines[1], Does.StartWith("**2.** __**Winning Publisher"));
            Assert.That(lines[1], Does.Contain(Trophy));
        }
    }

    private static LeagueYear BuildLeagueYear(int publisherCount, int? disconnectedDraftPosition, params (int DraftPosition, decimal Points)[] points)
    {
        var builder = new GetDraftStatusTestBuilder()
            .WithPublishers(publisherCount)
            .WithDraft(gamesToDraft: 1, counterPicksToDraft: 0, PlayStatus.DraftFinal);

        if (disconnectedDraftPosition.HasValue)
        {
            builder.DisconnectPublisher(disconnectedDraftPosition.Value);
        }

        foreach (var (draftPosition, publisherPoints) in points)
        {
            builder.SetFantasyPoints(draftPosition, publisherPoints);
        }

        return builder.Build();
    }
}
