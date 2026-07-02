using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using FantasyCritic.IntegrationTests.Tests.League.MultiDraft;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Draft.EdgeCases;

/// <summary>
/// Shared league presets and helpers for draft skip boundary tests (last standard/counter pick).
/// </summary>
internal static class DraftSkipBoundaryScenario
{
    /// <summary>2 players, 2 standard rounds + 1 counter-pick round per publisher.</summary>
    public static LeagueScenario TwoPlayerWithCounterPhase => new()
    {
        Name = "TwoPlayerSkipBoundaryWithCounter",
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

    /// <summary>2 players, 1 standard round each, no counter-pick phase.</summary>
    public static LeagueScenario TwoPlayerStandardOnly => new()
    {
        Name = "TwoPlayerSkipBoundaryStandardOnly",
        PlayerCount = 2,
        StandardGames = 1,
        GamesToDraft = 1,
        CounterPicks = 0,
        CounterPicksToDraft = 0,
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

    /// <summary>2 players, 1 standard round + 1 counter-pick round per publisher.</summary>
    public static LeagueScenario TwoPlayerOneStandardOneCounter => new()
    {
        Name = "TwoPlayerSkipBoundaryOneEach",
        PlayerCount = 2,
        StandardGames = 1,
        GamesToDraft = 1,
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

    public static Guid PublisherAtDraftPosition(LeagueYearViewModel snapshot, int draftPosition) =>
        snapshot.FirstDraft()!.PublisherDraftInfo.Single(i => i.DraftPosition == draftPosition).PublisherID;

    public static async Task<LeagueFixture> CreateLeagueWithPrefilledStandardSlotAsync(
        FantasyCriticWebApplicationFactory factory,
        LeagueScenario scenario,
        Func<(string email, string password, string displayName)> newUser,
        int draftPositionToPrefill)
    {
        var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(factory, scenario, newUser);
        var snapshot = await league.GetLeagueYearAsync();
        var publisherID = PublisherAtDraftPosition(snapshot, draftPositionToPrefill);
        await MultiDraftTestScenario.ManagerFillOneStandardSlotAsync(league, publisherID);
        await league.Manager.LeagueManager.StartDraftAsync(new StartDraftRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
        });
        return league;
    }

    /// <summary>
    /// Pre-fills one publisher's standard slot (that player auto-skips their standard turn), one
    /// publisher's counter slot, and starts the draft. At least one open standard slot must remain
    /// league-wide so <c>StartDraft</c> is allowed when <c>GamesToDraft &gt; 0</c>. Manager claims
    /// require no active draft.
    /// </summary>
    public static async Task<LeagueFixture> CreateLeagueWithPrefilledCounterSlotAsync(
        FantasyCriticWebApplicationFactory factory,
        LeagueScenario scenario,
        Func<(string email, string password, string displayName)> newUser,
        int draftPositionToPrefillCounter,
        int draftPositionToPrefillStandard = 1)
    {
        var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(factory, scenario, newUser);
        var snapshot = await league.GetLeagueYearAsync();
        var standardPublisherID = PublisherAtDraftPosition(snapshot, draftPositionToPrefillStandard);

        await MultiDraftTestScenario.ManagerFillOneStandardSlotAsync(league, standardPublisherID);

        var counterPublisherID = PublisherAtDraftPosition(snapshot, draftPositionToPrefillCounter);
        await ManagerFillOneCounterPickSlotAsync(league, counterPublisherID);

        await league.Manager.LeagueManager.StartDraftAsync(new StartDraftRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
        });
        return league;
    }

    public static async Task DraftCounterPicksAsync(LeagueFixture league, int count)
    {
        for (var pick = 0; pick < count; pick++)
        {
            var snapshot = await league.GetLeagueYearAsync();
            var activeDraft = snapshot.ActiveDraft();
            if (activeDraft is null || !activeDraft.DraftingCounterPicks)
            {
                throw new InvalidOperationException(
                    $"Cannot run counter pick {pick + 1} of {count}: draft is not in the counter-pick phase.");
            }

            var nextPublisherID = snapshot.Publishers.Single(p => p.NextToDraft).PublisherID;
            var testPublisher = league.Publishers.Single(p => p.PublisherID == nextPublisherID);
            var player = new MockedLivePlayer(testPublisher.Session, nextPublisherID, league.LeagueID);
            await player.DraftCounterPickAsync(league.Year);
        }
    }

    public static async Task ManagerFillOneCounterPickSlotAsync(LeagueFixture league, Guid publisherID)
    {
        var options = await league.Manager.League.PossibleCounterPicksAsync(publisherID);
        var pick = options.First();
        await league.Manager.LeagueManager.ManagerClaimGameAsync(new ClaimGameRequest
        {
            PublisherID = publisherID,
            GameName = pick.GameName,
            MasterGameID = pick.MasterGame!.MasterGameID,
            CounterPick = true,
            ManagerOverride = false,
            AllowIneligibleSlot = false,
        });
    }

    public static async Task ManualSkipCurrentPickAsync(LeagueFixture league)
    {
        await league.Manager.LeagueManager.SetDraftPauseAsync(new DraftPauseRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
            Pause = true,
        });

        var snapshot = await league.GetLeagueYearAsync();
        var draftID = snapshot.ActiveDraft()!.DraftID;

        await league.Manager.LeagueManager.SkipCurrentDraftPickAsync(new SkipCurrentDraftPickRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
            DraftID = draftID,
        });
    }

    public static void AssertDraftFinal(LeagueYearViewModel snapshot)
    {
        var draft = snapshot.Drafts.Single();
        Assert.That(draft.PlayStatus, Is.EqualTo("DraftFinal"),
            "Draft should be marked DraftFinal.");
        Assert.That(draft.DraftFinished, Is.True);
        Assert.That(snapshot.ActiveDraft(), Is.Null,
            "No draft should remain active after completion.");
    }

    public static async Task AssertTrailingAutoSkipPersistedAsync(LeagueFixture league, string slotTypePhrase)
    {
        var actions = await league.Manager.League.GetLeagueActionsAsync(league.LeagueID, league.Year);
        Assert.That(actions.Any(a =>
            a.ActionType == "Draft Pick Skipped"
            && a.Description.Contains("auto-skipped", StringComparison.OrdinalIgnoreCase)
            && a.Description.Contains(slotTypePhrase, StringComparison.OrdinalIgnoreCase)), Is.True,
            $"Expected a persisted auto-skip for the trailing {slotTypePhrase} turn.");
    }

    public static void AssertInCounterPickPhase(LeagueYearViewModel snapshot)
    {
        var draft = snapshot.ActiveDraft();
        Assert.That(draft, Is.Not.Null);
        Assert.That(draft!.DraftingCounterPicks, Is.True,
            "Draft should have advanced to the counter-pick phase.");
        Assert.That(draft.DraftFinished, Is.False);
    }
}

/// <summary>
/// Verifies draft phase transitions and completion when the last standard or counter pick is skipped.
/// </summary>
[TestFixture]
public class DraftSkipBoundaryTests : IntegrationTestBase
{
    [Test]
    public async Task LastStandardPick_AutoSkipped_ProgressesToCounterPicks()
    {
        // Snake order for 2×2 standard: R1 [P1, P2], R2 [P2, P1]. Pre-fill P1 so their R2 turn auto-skips.
        var league = await DraftSkipBoundaryScenario.CreateLeagueWithPrefilledStandardSlotAsync(
            Factory, DraftSkipBoundaryScenario.TwoPlayerWithCounterPhase, NewUser, draftPositionToPrefill: 1);
        try
        {
            await league.DraftUntilCounterPickPhaseAsync();
            var snapshot = await league.GetLeagueYearAsync();
            DraftSkipBoundaryScenario.AssertInCounterPickPhase(snapshot);
        }
        finally
        {
            await league.DisposeAsync();
        }
    }

    [Test]
    public async Task LastStandardPick_ManualSkipped_ProgressesToCounterPicks()
    {
        var league = await DraftSkipBoundaryScenario.CreateLeagueWithPrefilledStandardSlotAsync(
            Factory, DraftSkipBoundaryScenario.TwoPlayerWithCounterPhase, NewUser, draftPositionToPrefill: 1);
        try
        {
            // Three real picks before P1's final standard turn (P1 R1, P2 R1, P2 R2).
            await league.DraftStandardPicksAsync(3);
            await DraftSkipBoundaryScenario.ManualSkipCurrentPickAsync(league);

            var snapshot = await league.GetLeagueYearAsync();
            DraftSkipBoundaryScenario.AssertInCounterPickPhase(snapshot);
        }
        finally
        {
            await league.DisposeAsync();
        }
    }

    [Test]
    public async Task LastStandardPick_AutoSkipped_NoCounterPicks_DraftCompleted()
    {
        // Single standard round: R1 [P1, P2]. Pre-fill P2 so their only standard turn auto-skips.
        var league = await DraftSkipBoundaryScenario.CreateLeagueWithPrefilledStandardSlotAsync(
            Factory, DraftSkipBoundaryScenario.TwoPlayerStandardOnly, NewUser, draftPositionToPrefill: 2);
        try
        {
            await league.DraftStandardPicksAsync(1);
            var snapshot = await league.GetLeagueYearAsync();
            DraftSkipBoundaryScenario.AssertDraftFinal(snapshot);
            await DraftSkipBoundaryScenario.AssertTrailingAutoSkipPersistedAsync(league, "standard game");
        }
        finally
        {
            await league.DisposeAsync();
        }
    }

    [Test]
    public async Task LastStandardPick_ManualSkipped_NoCounterPicks_DraftCompleted()
    {
        var league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, DraftSkipBoundaryScenario.TwoPlayerStandardOnly, NewUser);
        try
        {
            await league.DraftStandardPicksAsync(1);
            await DraftSkipBoundaryScenario.ManualSkipCurrentPickAsync(league);

            var snapshot = await league.GetLeagueYearAsync();
            DraftSkipBoundaryScenario.AssertDraftFinal(snapshot);
        }
        finally
        {
            await league.DisposeAsync();
        }
    }

    [Test]
    public async Task LastCounterPick_AutoSkipped_DraftCompleted()
    {
        // P1's standard slot is pre-filled (auto-skip); P2 drafts the lone standard game; P2's counter
        // slot is pre-filled before the draft; P1's counter pick completes the draft while P2 auto-skips.
        var league = await DraftSkipBoundaryScenario.CreateLeagueWithPrefilledCounterSlotAsync(
            Factory, DraftSkipBoundaryScenario.TwoPlayerOneStandardOneCounter, NewUser,
            draftPositionToPrefillCounter: 2, draftPositionToPrefillStandard: 1);
        try
        {
            await league.DraftUntilCounterPickPhaseAsync();
            await DraftSkipBoundaryScenario.DraftCounterPicksAsync(league, 1);

            var snapshot = await league.GetLeagueYearAsync();
            DraftSkipBoundaryScenario.AssertDraftFinal(snapshot);
            await DraftSkipBoundaryScenario.AssertTrailingAutoSkipPersistedAsync(league, "counter-pick");
        }
        finally
        {
            await league.DisposeAsync();
        }
    }

    [Test]
    public async Task LastCounterPick_ManualSkipped_DraftCompleted()
    {
        var league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, DraftSkipBoundaryScenario.TwoPlayerOneStandardOneCounter, NewUser);
        try
        {
            await league.DraftUntilCounterPickPhaseAsync();
            await DraftSkipBoundaryScenario.DraftCounterPicksAsync(league, 1);
            await DraftSkipBoundaryScenario.ManualSkipCurrentPickAsync(league);

            var snapshot = await league.GetLeagueYearAsync();
            DraftSkipBoundaryScenario.AssertDraftFinal(snapshot);
        }
        finally
        {
            await league.DisposeAsync();
        }
    }
}
