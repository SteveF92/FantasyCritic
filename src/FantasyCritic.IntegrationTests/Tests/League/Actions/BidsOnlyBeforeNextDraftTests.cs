using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Actions;

/// <summary>
/// Exercises BidsOnlyBeforeNextScheduledDraft through the full HTTP stack:
/// multi-draft league with draft 1 complete and draft 2 pending → MakePickupBid.
/// </summary>
[TestFixture]
public class BidsOnlyBeforeNextDraftTests : IntegrationTestBase
{
    private const int UnknownMaximumReleaseYear = 9999;

    private static readonly LeagueScenario BidsOnlyScenario = new()
    {
        Name = "BidsOnlyBeforeNextDraft",
        PlayerCount = 2,
        StandardGames = 4,
        CounterPicks = 1,
        EnableBids = true,
        BidsOnlyBeforeNextScheduledDraft = true,
        DraftSystem = "Flexible",
        PickupSystem = "SemiPublicBiddingSecretCounterPicks",
        ScoringSystem = "LinearPositive",
        TradingSystem = "Standard",
        TiebreakSystem = "LowestProjectedPoints",
        ReleaseSystem = "MustBeReleased",
        IneligibleGameSystem = "DroppableAsWillNotRelease",
    };

    private sealed record LeagueContext(
        ApiSession AdminSession,
        TestPublisher Publisher1,
        TestPublisher Publisher2,
        Guid LeagueID,
        int Year,
        List<ApiSession> OwnedSessions)
    {
        public async ValueTask DisposeAsync()
        {
            await AdminSession.Admin.ResetTimeAsync();
            foreach (var session in OwnedSessions)
                session.Dispose();
            AdminSession.Dispose();
        }
    }

    [Test]
    public async Task BidAccepted_WhenMaxReleaseBeforeScheduledDraft()
    {
        var ctx = await SetUpLeagueAfterDraft1Async(new DateTimeOffset(2099, 12, 31, 0, 0, 0, TimeSpan.Zero));
        try
        {
            var available = await ctx.Publisher1.Session.League.TopAvailableGamesAsync(
                ctx.Year, ctx.LeagueID, ctx.Publisher1.PublisherID, null);
            var game = available.FirstOrDefault(g => g.IsAvailable && !g.Taken && !g.IsReleased)
                ?? throw new InvalidOperationException("No available unreleased game found for bid placement.");

            var result = await ctx.Publisher1.Session.League.MakePickupBidAsync(new PickupBidRequest
            {
                PublisherID = ctx.Publisher1.PublisherID,
                MasterGameID = game.MasterGame.MasterGameID,
                CounterPick = false,
                BidAmount = 10,
                AllowIneligibleSlot = false,
            });

            Assert.That(result.Success, Is.True,
                () => $"Expected bid to succeed. Errors: {string.Join("; ", result.Errors ?? [])}");
        }
        finally
        {
            await ctx.DisposeAsync();
        }
    }

    [Test]
    public async Task BidRejected_WhenMaxReleaseDateIsNull()
    {
        var ctx = await SetUpLeagueAfterDraft1Async(new DateTimeOffset(2025, 1, 7, 0, 0, 0, TimeSpan.Zero));
        try
        {
            var available = await ctx.Publisher1.Session.League.TopAvailableGamesAsync(
                ctx.Year, ctx.LeagueID, ctx.Publisher1.PublisherID, null);

            var game = available.FirstOrDefault(g =>
                g.IsAvailable && !g.Taken && !g.IsReleased && HasUnknownMaximumReleaseDate(g));

            if (game is null)
            {
                Assert.Inconclusive(
                    "Seed data has no available game with an unknown maximum release date (TBA / open-ended).");
                return;
            }

            var result = await ctx.Publisher1.Session.League.MakePickupBidAsync(new PickupBidRequest
            {
                PublisherID = ctx.Publisher1.PublisherID,
                MasterGameID = game.MasterGame.MasterGameID,
                CounterPick = false,
                BidAmount = 10,
                AllowIneligibleSlot = false,
            });

            Assert.That(result.Success, Is.False);
            Assert.That(result.Errors.Any(e => e.Contains("only allows bids", StringComparison.OrdinalIgnoreCase)), Is.True,
                () => $"Expected bids-only error. Actual errors: {string.Join("; ", result.Errors ?? [])}");
        }
        finally
        {
            await ctx.DisposeAsync();
        }
    }

    [Test]
    public async Task BidRejected_WhenMaxReleaseOnOrAfterDraftDate()
    {
        var draftDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var ctx = await SetUpLeagueAfterDraft1Async(draftDate);
        try
        {
            var available = await ctx.Publisher1.Session.League.TopAvailableGamesAsync(
                ctx.Year, ctx.LeagueID, ctx.Publisher1.PublisherID, null);

            var game = available.FirstOrDefault(g =>
                g.IsAvailable && !g.Taken && !g.IsReleased
                && g.MasterGame.MaximumReleaseDate.Date >= draftDate.Date
                && !HasUnknownMaximumReleaseDate(g));

            if (game is null)
            {
                Assert.Inconclusive(
                    $"Seed data has no available game with maximum release date on or after {draftDate:yyyy-MM-dd}.");
                return;
            }

            var result = await ctx.Publisher1.Session.League.MakePickupBidAsync(new PickupBidRequest
            {
                PublisherID = ctx.Publisher1.PublisherID,
                MasterGameID = game.MasterGame.MasterGameID,
                CounterPick = false,
                BidAmount = 10,
                AllowIneligibleSlot = false,
            });

            Assert.That(result.Success, Is.False);
            Assert.That(result.Errors.Any(e => e.Contains("only allows bids", StringComparison.OrdinalIgnoreCase)), Is.True,
                () => $"Expected bids-only error. Actual errors: {string.Join("; ", result.Errors ?? [])}");
        }
        finally
        {
            await ctx.DisposeAsync();
        }
    }

    private async Task<LeagueContext> SetUpLeagueAfterDraft1Async(DateTimeOffset draft2ScheduledDate)
    {
        var ownedSessions = new List<ApiSession>();

        var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);
        await adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 6, 12, 0, 0, TimeSpan.Zero),
        });

        var (mgrEmail, mgrPassword, mgrDisplayName) = NewUser();
        var manager = new ApiSession(Factory);
        ownedSessions.Add(manager);
        await manager.RegisterAsync(mgrEmail, mgrPassword, mgrDisplayName);

        var (p2Email, p2Password, p2DisplayName) = NewUser();
        var player2 = new ApiSession(Factory);
        ownedSessions.Add(player2);
        await player2.RegisterAsync(p2Email, p2Password, p2DisplayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(manager);
        var leagueID = await manager.LeagueManager.CreateLeagueAsync(new CreateLeagueRequest
        {
            LeagueName = $"TestLeague-{Guid.NewGuid():N}"[..30],
            PublicLeague = false,
            TestLeague = true,
            CustomRulesLeague = false,
            LeagueYearSettings = BidsOnlyScenario.BuildSettings(year),
            Drafts = new List<DraftSettingsRequest>
            {
                new() { GamesToDraft = 2, CounterPicksToDraft = 1, ScheduledDate = null },
                new()
                {
                    Name = "Draft 2",
                    GamesToDraft = 2,
                    CounterPicksToDraft = 0,
                    ScheduledDate = draft2ScheduledDate,
                },
            },
        });

        await LeagueTestHelpers.InviteAndAcceptAsync(manager, player2, leagueID);

        var pub1ID = await LeagueTestHelpers.CreatePublisherAsync(manager, leagueID, year, "Manager Publisher");
        var pub2ID = await LeagueTestHelpers.CreatePublisherAsync(player2, leagueID, year, "Player 2 Publisher");

        await LeagueTestHelpers.SetDraftOrderAsync(manager, leagueID, year, [pub1ID, pub2ID]);

        await manager.LeagueManager.StartDraftAsync(new StartDraftRequest
        {
            LeagueID = leagueID,
            Year = year,
        });

        var publisher1 = new TestPublisher(1, manager, pub1ID, "Manager Publisher");
        var publisher2 = new TestPublisher(2, player2, pub2ID, "Player 2 Publisher");

        var simulator = new DraftSimulator(manager, [
            new MockedLivePlayer(manager, pub1ID, leagueID),
            new MockedLivePlayer(player2, pub2ID, leagueID),
        ]);
        await simulator.RunAsync(leagueID, year);

        var snapshot = await manager.League.GetLeagueYearAsync(leagueID, year, null);
        var draft1 = snapshot.Drafts.Single(d => d.DraftNumber == 1);
        var draft2 = snapshot.Drafts.Single(d => d.DraftNumber == 2);

        Assert.That(draft1.PlayStatus, Is.EqualTo("DraftFinal"),
            "Draft 1 should be complete before bid placement tests run.");
        Assert.That(draft2.PlayStatus, Is.EqualTo("NotStartedDraft"),
            "Draft 2 should remain pending so BidsOnlyBeforeNextScheduledDraft applies.");
        Assert.That(draft2.ScheduledDate, Is.Not.Null);
        Assert.That(draft2.ScheduledDate!.Value.Date, Is.EqualTo(draft2ScheduledDate.Date));

        return new LeagueContext(adminSession, publisher1, publisher2, leagueID, year, ownedSessions);
    }

    /// <summary>
    /// The API exposes unknown maximum release dates via TBA estimated dates or the
    /// definite-max sentinel (LocalDate.MaxIsoValue → year 9999).
    /// </summary>
    private static bool HasUnknownMaximumReleaseDate(PossibleMasterGameYearViewModel game) =>
        string.Equals(game.MasterGame.EstimatedReleaseDate, "TBA", StringComparison.OrdinalIgnoreCase)
        || game.MasterGame.MaximumReleaseDate.Year >= UnknownMaximumReleaseYear;
}
