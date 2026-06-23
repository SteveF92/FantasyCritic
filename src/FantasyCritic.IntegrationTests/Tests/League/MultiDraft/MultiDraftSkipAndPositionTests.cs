using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.MultiDraft;

/// <summary>
/// Shared scenario + helpers for multi-draft execution edge cases (per-draft positions and the
/// auto-skip mechanic). All scenarios use a 2-player league so draft state is easy to reason about.
/// </summary>
internal static class MultiDraftTestScenario
{
    public static LeagueScenario TwoPlayer => new LeagueScenario
    {
        Name = "TwoPlayerMultiDraftEdge",
        PlayerCount = 2,
        StandardGames = 2,
        GamesToDraft = 2,
        CounterPicks = 1,
        CounterPicksToDraft = 1,
        UnrestrictedReleaseStatusDroppableGames = 0,
        WillNotReleaseDroppableGames = 0,
        WillReleaseDroppableGames = 0,
        DropOnlyDraftGames = true,
        GrantSuperDrops = false,
        CounterPicksBlockDrops = false,
        AllowMoveIntoIneligible = false,
        MinimumBidAmount = 0,
        EnableBids = false,
        DraftSystem = "Flexible",
        PickupSystem = "SemiPublicBiddingSecretCounterPicks",
        ScoringSystem = "LinearPositive",
        TradingSystem = "NoTrades",
        TiebreakSystem = "LowestProjectedPoints",
        ReleaseSystem = "OnlyNeedsScore",
        IneligibleGameSystem = "DroppableAsWillNotRelease",
    };

    public static async Task CreateSecondDraftAsync(
        LeagueFixture league, int gamesToDraft, int counterPicksToDraft, int additionalStandardGames, int additionalCounterPicks)
    {
        await league.Manager.LeagueManager.CreateLeagueDraftAsync(new CreateLeagueDraftRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
            Name = "Draft 2",
            ScheduledDate = null,
            GamesToDraft = gamesToDraft,
            CounterPicksToDraft = counterPicksToDraft,
            AdditionalStandardGames = additionalStandardGames,
            AdditionalCounterPicks = additionalCounterPicks,
            NewSpecialGameSlots = new List<SpecialGameSlotViewModel>(),
        });
    }

    public static async Task SetDraft2OrderAsync(LeagueFixture league)
    {
        var snapshot = await league.GetLeagueYearAsync();
        var draft2 = snapshot.Drafts.Single(d => d.DraftNumber == 2);
        await league.Manager.LeagueManager.SetDraftOrderAsync(new DraftOrderRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
            DraftID = draft2.DraftID,
            DraftOrderType = "Random",
            ManualPublisherDraftPositions = null,
        });
    }

    public static async Task StartDraft2Async(LeagueFixture league)
    {
        await league.Manager.LeagueManager.StartDraftAsync(new StartDraftRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
        });
    }

    /// <summary>
    /// Uses the manager to manually assign one standard game to the given publisher while no draft
    /// is active. The game is acquired with a null DraftID (it is not a draft pick), which fills a
    /// roster slot — mimicking a slot filled by a bid between drafts.
    /// </summary>
    public static async Task ManagerFillOneStandardSlotAsync(LeagueFixture league, Guid publisherID)
    {
        var available = await league.Manager.League.TopAvailableGamesAsync(
            league.Year, league.LeagueID, publisherID, null);
        var game = available.First(g => g.IsAvailable && !g.Taken && !g.IsReleased);
        await league.Manager.LeagueManager.ManagerClaimGameAsync(new ClaimGameRequest
        {
            PublisherID = publisherID,
            GameName = game.MasterGame.GameName,
            MasterGameID = game.MasterGame.MasterGameID,
            CounterPick = false,
            ManagerOverride = false,
            AllowIneligibleSlot = false,
        });
    }
}

/// <summary>
/// Concern 1: DraftPosition / OverallDraftPosition should be per-draft and reset to start at 1 for
/// each draft. Verified via the stored OverallDraftPosition on each publisher game.
/// </summary>
[TestFixture]
public class MultiDraftPositionResetTests : IntegrationTestBase
{
    private LeagueFixture _league = null!;
    private HashSet<Guid> _draft1GameIDs = null!;
    private LeagueYearViewModel _finalSnapshot = null!;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, MultiDraftTestScenario.TwoPlayer, NewUser);
        await _league.DraftToCompletionAsync();

        var afterDraft1 = await _league.GetLeagueYearAsync();
        _draft1GameIDs = afterDraft1.Publishers.SelectMany(p => p.Games).Select(g => g.PublisherGameID).ToHashSet();

        await MultiDraftTestScenario.CreateSecondDraftAsync(_league, gamesToDraft: 2, counterPicksToDraft: 1, additionalStandardGames: 2, additionalCounterPicks: 1);
        await MultiDraftTestScenario.SetDraft2OrderAsync(_league);
        await MultiDraftTestScenario.StartDraft2Async(_league);
        await _league.DraftToCompletionAsync();

        _finalSnapshot = await _league.GetLeagueYearAsync();
    }

    [OneTimeTearDown]
    public async Task TearDown() => await _league.DisposeAsync();

    private IReadOnlyList<int> StandardPositions(bool draft2) =>
        _finalSnapshot.Publishers.SelectMany(p => p.Games)
            .Where(g => !g.CounterPick && _draft1GameIDs.Contains(g.PublisherGameID) != draft2)
            .Select(g => g.OverallDraftPosition!.Value)
            .OrderBy(x => x)
            .ToList();

    private IReadOnlyList<int> CounterPickPositions(bool draft2) =>
        _finalSnapshot.Publishers.SelectMany(p => p.Games)
            .Where(g => g.CounterPick && _draft1GameIDs.Contains(g.PublisherGameID) != draft2)
            .Select(g => g.OverallDraftPosition!.Value)
            .OrderBy(x => x)
            .ToList();

    [Test]
    public void Draft1_StandardOverallPositions_AreOneThroughFour()
    {
        Assert.That(StandardPositions(draft2: false), Is.EqualTo(new[] { 1, 2, 3, 4 }));
    }

    [Test]
    public void Draft2_StandardOverallPositions_ResetToOneThroughFour()
    {
        Assert.That(StandardPositions(draft2: true), Is.EqualTo(new[] { 1, 2, 3, 4 }),
            "Draft 2 standard OverallDraftPositions should reset to 1..4, not continue from draft 1.");
    }

    [Test]
    public void Draft1_CounterPickOverallPositions_AreOneTwo()
    {
        Assert.That(CounterPickPositions(draft2: false), Is.EqualTo(new[] { 1, 2 }));
    }

    [Test]
    public void Draft2_CounterPickOverallPositions_ResetToOneTwo()
    {
        Assert.That(CounterPickPositions(draft2: true), Is.EqualTo(new[] { 1, 2 }),
            "Draft 2 counter-pick OverallDraftPositions should reset to 1..2, not continue from draft 1.");
    }
}

/// <summary>
/// Concerns 2 &amp; 3: the auto-skip mechanic and draft completion when publishers have an unequal
/// number of open slots in a draft (one slot pre-filled by a manager claim between drafts, standing
/// in for a between-draft bid win).
///
/// Both tests are currently <c>[Ignore]</c>d because they are expected to fail against the current
/// implementation — they document the suspected bugs so we can un-ignore them as we fix the logic:
///   - The auto-skip check compares draft-specific game count against <c>GamesToDraft</c> rather than
///     against the publisher's open roster slots, so a publisher with a pre-filled slot is asked to
///     draft more games than they have room for.
///   - <c>CompleteDraft</c> expects exactly <c>GamesToDraft * PublisherCount</c> games, so a draft in
///     which any publisher legitimately drafts fewer games never registers as complete.
/// </summary>
[TestFixture]
public class MultiDraftSkipExecutionTests : IntegrationTestBase
{
    /// <summary>
    /// Builds a 2-player league, completes draft 1, creates a standard-only second draft (+2 standard
    /// slots), fills one of publisher[0]'s new slots via a manager claim, and sets the draft-2 order.
    /// Draft 2 is left un-started so the caller can start it (auto vs manual).
    /// </summary>
    private async Task<LeagueFixture> BuildLeagueWithUnequalDraft2SlotsAsync()
    {
        var league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, MultiDraftTestScenario.TwoPlayer, NewUser);
        await league.DraftToCompletionAsync();

        await MultiDraftTestScenario.CreateSecondDraftAsync(league, gamesToDraft: 2, counterPicksToDraft: 0, additionalStandardGames: 2, additionalCounterPicks: 0);
        await MultiDraftTestScenario.ManagerFillOneStandardSlotAsync(league, league.Publishers[0].PublisherID);
        await MultiDraftTestScenario.SetDraft2OrderAsync(league);

        return league;
    }

    [Test]
    [Ignore("Exposes suspected bug (concerns 2 & 3): auto-skip uses the GamesToDraft quota rather than open roster slots, and CompleteDraft expects GamesToDraft*PublisherCount games. A draft where one publisher has a pre-filled slot never completes. Confirmed failing (draft never reaches DraftFinal). Un-ignore once the skip/complete logic is fixed.")]
    public async Task AutoDraft_WithUnequalSlots_SkipsFullPublisherAndCompletes()
    {
        var league = await BuildLeagueWithUnequalDraft2SlotsAsync();
        try
        {
            foreach (var p in league.Publishers)
            {
                await p.Session.League.SetAutoDraftAsync(new SetAutoDraftRequest
                {
                    PublisherID = p.PublisherID,
                    Mode = "All",
                    OnlyDraftFromWatchlist = false,
                });
            }

            await MultiDraftTestScenario.StartDraft2Async(league);

            var snapshot = await league.GetLeagueYearAsync();
            Assert.That(snapshot.Drafts.All(d => d.PlayStatus == "DraftFinal"), Is.True,
                "Draft 2 should complete even though publisher[0] had only one open standard slot.");
            foreach (var p in snapshot.Publishers)
            {
                Assert.That(p.Games.Count(g => !g.CounterPick), Is.EqualTo(4),
                    $"{p.PublisherName} should end with a full roster of 4 standard games.");
            }
        }
        finally
        {
            await league.DisposeAsync();
        }
    }

    [Test]
    [Ignore("Exposes suspected bug (concern 2, manual path): there is no skip handling in the live/manual draft path, so a publisher with no open slots stays 'next to draft' and cannot complete their turn. Confirmed failing (draft simulator throws because the full publisher has no draftable candidate). Un-ignore once the manual skip logic is implemented.")]
    public async Task ManualDraft_WithUnequalSlots_SkipsFullPublisherAndCompletes()
    {
        var league = await BuildLeagueWithUnequalDraft2SlotsAsync();
        try
        {
            await MultiDraftTestScenario.StartDraft2Async(league);
            await league.DraftToCompletionAsync();

            var snapshot = await league.GetLeagueYearAsync();
            Assert.That(snapshot.Drafts.All(d => d.PlayStatus == "DraftFinal"), Is.True,
                "Draft 2 should complete with publisher[0] skipped on the turn they have no open slot for.");
            foreach (var p in snapshot.Publishers)
            {
                Assert.That(p.Games.Count(g => !g.CounterPick), Is.EqualTo(4),
                    $"{p.PublisherName} should end with a full roster of 4 standard games.");
            }
        }
        finally
        {
            await league.DisposeAsync();
        }
    }
}
