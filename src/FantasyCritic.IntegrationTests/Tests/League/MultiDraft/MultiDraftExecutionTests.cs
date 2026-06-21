using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.MultiDraft;

/// <summary>
/// Verifies that after draft 1 completes normally in a multi-draft-capable league
/// the publishers have exactly the games they should, and the draft is marked final.
/// </summary>
[TestFixture]
public class MultiDraftDraft1CompletionTests : IntegrationTestBase
{
    private LeagueFixture _league = null!;
    private LeagueYearViewModel _snapshot = null!;

    private static LeagueScenario TwoPlayerScenario => new LeagueScenario
    {
        Name = "TwoPlayerMultiDraft",
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

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, TwoPlayerScenario, NewUser);
        await _league.DraftToCompletionAsync();
        _snapshot = await _league.GetLeagueYearAsync();
    }

    [OneTimeTearDown]
    public async Task TearDown() => await _league.DisposeAsync();

    [Test]
    public void Draft1_Completes_AllPublishersHave2StandardGames()
    {
        foreach (var p in _snapshot.Publishers)
        {
            Assert.That(p.Games.Count(g => !g.CounterPick), Is.EqualTo(2),
                $"Publisher {p.PublisherName} should have 2 standard games after draft 1.");
        }
    }

    [Test]
    public void Draft1_Completes_AllPublishersHave1CounterPick()
    {
        foreach (var p in _snapshot.Publishers)
        {
            Assert.That(p.Games.Count(g => g.CounterPick), Is.EqualTo(1),
                $"Publisher {p.PublisherName} should have 1 counter-pick after draft 1.");
        }
    }

    [Test]
    public void Draft1_Completes_FirstDraftIsMarkedDraftFinal()
    {
        var draft1 = _snapshot.Drafts.Single(d => d.DraftNumber == 1);
        Assert.That(draft1.PlayStatus, Is.EqualTo("DraftFinal"));
    }
}

/// <summary>
/// Creates a second draft, sets its order, starts it, and runs it to completion.
/// Verifies that both drafts end up DraftFinal and publishers accumulate games from both drafts.
/// </summary>
[TestFixture]
public class MultiDraftSecondDraftExecutionTests : IntegrationTestBase
{
    private LeagueFixture _league = null!;
    private LeagueYearViewModel _finalSnapshot = null!;

    private static LeagueScenario TwoPlayerScenario => new LeagueScenario
    {
        Name = "TwoPlayerMultiDraft",
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

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, TwoPlayerScenario, NewUser);
        await _league.DraftToCompletionAsync();

        // Create a second draft adding 2 more std slots + 1 more CP slot per publisher.
        await _league.Manager.LeagueManager.CreateLeagueDraftAsync(new CreateLeagueDraftRequest
        {
            LeagueID = _league.LeagueID,
            Year = _league.Year,
            Name = "Draft 2",
            ScheduledDate = null,
            GamesToDraft = 2,
            CounterPicksToDraft = 1,
            AdditionalStandardGames = 2,
            AdditionalCounterPicks = 1,
            NewSpecialGameSlots = new List<SpecialGameSlotViewModel>(),
        });

        var preSnapshot = await _league.GetLeagueYearAsync();
        var draft2 = preSnapshot.Drafts.Single(d => d.DraftNumber == 2);

        // Set a random draft order for draft 2.
        await _league.Manager.LeagueManager.SetDraftOrderAsync(new DraftOrderRequest
        {
            LeagueID = _league.LeagueID,
            Year = _league.Year,
            DraftID = draft2.DraftID,
            DraftOrderType = "Random",
            ManualPublisherDraftPositions = null,
        });

        // Start draft 2 (targets PendingDraft automatically).
        await _league.Manager.LeagueManager.StartDraftAsync(new StartDraftRequest
        {
            LeagueID = _league.LeagueID,
            Year = _league.Year,
        });

        // Run draft 2 to completion using the existing simulator.
        await _league.DraftToCompletionAsync();

        _finalSnapshot = await _league.GetLeagueYearAsync();
    }

    [OneTimeTearDown]
    public async Task TearDown() => await _league.DisposeAsync();

    [Test]
    public void BothDrafts_AreMarkedDraftFinal()
    {
        Assert.That(_finalSnapshot.Drafts.All(d => d.PlayStatus == "DraftFinal"), Is.True,
            "All drafts should be DraftFinal after the second draft completes.");
    }

    [Test]
    public void AllPublishers_Have4StandardGamesAfterBothDrafts()
    {
        foreach (var p in _finalSnapshot.Publishers)
        {
            Assert.That(p.Games.Count(g => !g.CounterPick), Is.EqualTo(4),
                $"Publisher {p.PublisherName} should have 4 standard games after both drafts.");
        }
    }

    [Test]
    public void AllPublishers_Have2CounterPicksAfterBothDrafts()
    {
        foreach (var p in _finalSnapshot.Publishers)
        {
            Assert.That(p.Games.Count(g => g.CounterPick), Is.EqualTo(2),
                $"Publisher {p.PublisherName} should have 2 counter-picks after both drafts.");
        }
    }
}
