using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Actions;

/// <summary>
/// Tests that bids and drops which were eligible when placed are correctly
/// rejected at processing time when the target (or conditionally-dropped) game
/// gains a critic score in the intervening window.
/// </summary>
[TestFixture]
public class EligibilityChangeTests : IntegrationTestBase
{
    private sealed record EligibilityTargets(
        Guid GameA,
        Guid GameB,
        Guid GameE,
        Guid GameG,
        Guid P2CounterPickTarget,
        Guid P3StandaloneDropPublisherGameID,
        Guid P3StandaloneDropMasterGameID,
        Guid P3ConditionalDropPublisherGameID,
        Guid P3ConditionalDropMasterGameID,
        Guid P4StandaloneDropPublisherGameID,
        Guid P4StandaloneDropMasterGameID,
        Guid P4ConditionalDropPublisherGameID,
        Guid P4ConditionalDropMasterGameID);

    private ApiSession _adminSession = null!;
    private LeagueFixture _league = null!;
    private EligibilityTargets _targets = null!;

    private int _p1StartBudget;
    private int _p2StartBudget;

    private LeagueYearViewModel _postProcessingSnapshot = null!;
    private LeagueActionProcessingSetViewModel _actionSet = null!;

    [OneTimeSetUp]
    public async Task SetUpEligibilityChangeLeague()
    {
        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);

        await _adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 6, 12, 0, 0, TimeSpan.Zero)
        });

        _league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, LeagueScenarios.FourPlayerEligibilityChange, NewUser);
        await _league.DraftToCompletionAsync();

        var postDraftSnapshot = await _league.GetLeagueYearAsync();
        _p1StartBudget = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[0].PublisherID).Budget;
        _p2StartBudget = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[1].PublisherID).Budget;

        var p3Publisher = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[2].PublisherID);
        var p4Publisher = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[3].PublisherID);

        var usedMasterGameIDs = new HashSet<Guid>();

        var p1Available = await _league.Publishers[0].Session.League.TopAvailableGamesAsync(
            _league.Year, _league.LeagueID, _league.Publishers[0].PublisherID, null);
        var gameA = await SelectBidTargetAsync(p1Available, usedMasterGameIDs);
        usedMasterGameIDs.Add(gameA);

        var p2Available = await _league.Publishers[1].Session.League.TopAvailableGamesAsync(
            _league.Year, _league.LeagueID, _league.Publishers[1].PublisherID, null);
        var gameB = await SelectBidTargetAsync(p2Available, usedMasterGameIDs);
        usedMasterGameIDs.Add(gameB);

        var p3Available = await _league.Publishers[2].Session.League.TopAvailableGamesAsync(
            _league.Year, _league.LeagueID, _league.Publishers[2].PublisherID, null);
        var gameE = await SelectBidTargetAsync(p3Available, usedMasterGameIDs);
        usedMasterGameIDs.Add(gameE);

        var p4Available = await _league.Publishers[3].Session.League.TopAvailableGamesAsync(
            _league.Year, _league.LeagueID, _league.Publishers[3].PublisherID, null);
        var gameG = await SelectBidTargetAsync(p4Available, usedMasterGameIDs);

        var possibleCPs = await _league.Publishers[1].Session.League.PossibleCounterPicksAsync(
            _league.Publishers[1].PublisherID);
        var p2CounterPickTarget = await SelectCounterPickTargetAsync(possibleCPs);

        var p3DroppableGames = p3Publisher.Games
            .Where(g => !g.CounterPick && !g.DropBlocked
                && g.MasterGame != null && g.OverallDraftPosition.HasValue)
            .ToList();
        if (p3DroppableGames.Count < 2)
            throw new InvalidOperationException(
                "P3 needs at least 2 droppable drafted games for the eligibility-change test setup.");

        var p4DroppableGames = p4Publisher.Games
            .Where(g => !g.CounterPick && !g.DropBlocked
                && g.MasterGame != null && g.OverallDraftPosition.HasValue)
            .ToList();
        if (p4DroppableGames.Count < 2)
            throw new InvalidOperationException(
                "P4 needs at least 2 droppable drafted games for the eligibility-change test setup.");

        _targets = new EligibilityTargets(
            GameA: gameA,
            GameB: gameB,
            GameE: gameE,
            GameG: gameG,
            P2CounterPickTarget: p2CounterPickTarget,
            P3StandaloneDropPublisherGameID: p3DroppableGames[0].PublisherGameID,
            P3StandaloneDropMasterGameID: p3DroppableGames[0].MasterGame!.MasterGameID,
            P3ConditionalDropPublisherGameID: p3DroppableGames[1].PublisherGameID,
            P3ConditionalDropMasterGameID: p3DroppableGames[1].MasterGame!.MasterGameID,
            P4StandaloneDropPublisherGameID: p4DroppableGames[0].PublisherGameID,
            P4StandaloneDropMasterGameID: p4DroppableGames[0].MasterGame!.MasterGameID,
            P4ConditionalDropPublisherGameID: p4DroppableGames[1].PublisherGameID,
            P4ConditionalDropMasterGameID: p4DroppableGames[1].MasterGame!.MasterGameID);

        await PlaceBidAsync(_league.Publishers[0], _targets.GameA, 10, false, null);
        await PlaceBidAsync(_league.Publishers[1], _targets.GameB, 10, false, null);
        await PlaceDropAsync(_league.Publishers[2], _targets.P3StandaloneDropPublisherGameID);
        await PlaceBidAsync(_league.Publishers[2], _targets.GameE, 10, false, _targets.P3ConditionalDropPublisherGameID);
        await PlaceDropAsync(_league.Publishers[3], _targets.P4StandaloneDropPublisherGameID);
        await PlaceBidAsync(_league.Publishers[3], _targets.GameG, 10, false, _targets.P4ConditionalDropPublisherGameID);
        await PlaceBidAsync(_league.Publishers[1], _targets.P2CounterPickTarget, 5, true, null);

        await _adminSession.Admin.SetTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 10, 20, 0, 0, TimeSpan.Zero)
        });

        await EditMasterGameToAddScoreAsync(_targets.GameB);
        await EditMasterGameToAddScoreAsync(_targets.GameG);
        await EditMasterGameToAddScoreAsync(_targets.P4StandaloneDropMasterGameID);
        await EditMasterGameToAddScoreAsync(_targets.P4ConditionalDropMasterGameID);
        await EditMasterGameToAddScoreAsync(_targets.P2CounterPickTarget);

        await _adminSession.Admin.SetTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 12, 1, 1, 0, TimeSpan.Zero)
        });

        await _adminSession.ActionRunner.TurnOnActionProcessingModeAsync();
        await _adminSession.ActionRunner.ProcessActionsAsync();
        await _adminSession.ActionRunner.TurnOffActionProcessingModeAsync();

        _postProcessingSnapshot = await _adminSession.League.GetLeagueYearAsync(_league.LeagueID, _league.Year, null);
        var actionSets = await _adminSession.League.GetLeagueActionSetsAsync(_league.LeagueID, _league.Year);
        _actionSet = actionSets.Single();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        if (_adminSession != null && _targets != null)
        {
            await SafeResetMasterGameScoreAsync(_targets.GameB);
            await SafeResetMasterGameScoreAsync(_targets.GameG);
            await SafeResetMasterGameScoreAsync(_targets.P4StandaloneDropMasterGameID);
            await SafeResetMasterGameScoreAsync(_targets.P4ConditionalDropMasterGameID);
            await SafeResetMasterGameScoreAsync(_targets.P2CounterPickTarget);

            await _adminSession.ActionRunner.TurnOffActionProcessingModeAsync();
            await _adminSession.Admin.ResetTimeAsync();
        }
        _adminSession?.Dispose();
        if (_league != null)
            await _league.DisposeAsync();
    }

    private async Task EditMasterGameToAddScoreAsync(Guid masterGameID)
    {
        var game = await _adminSession.Game.MasterGameAsync(masterGameID);
        await _adminSession.FactChecker.EditMasterGameAsync(new EditMasterGameRequest
        {
            MasterGameID = masterGameID,
            GameName = game.GameName,
            EstimatedReleaseDate = game.EstimatedReleaseDate,
            MinimumReleaseDate = game.MinimumReleaseDate,
            MaximumReleaseDate = game.MaximumReleaseDate,
            EarlyAccessReleaseDate = game.EarlyAccessReleaseDate,
            InternationalReleaseDate = game.InternationalReleaseDate,
            AnnouncementDate = game.AnnouncementDate,
            ReleaseDate = game.ReleaseDate,
            Tags = game.Tags.ToList(),
            SyncWithExternalAPIs = game.SyncWithExternalAPIs,
            UseSimpleEligibility = game.UseSimpleEligibility,
            DelayContention = game.DelayContention,
            ShowNote = game.ShowNote,
            OpenCriticID = game.OpenCriticID,
            GgToken = game.GgToken,
            Notes = game.Notes,
            CriticScore = 85.0m,
        });
    }

    private async Task SafeResetMasterGameScoreAsync(Guid masterGameID)
    {
        if (masterGameID == Guid.Empty)
            return;
        try
        {
            var game = await _adminSession.Game.MasterGameAsync(masterGameID);
            if (game.CriticScore == null)
                return;
            await _adminSession.FactChecker.EditMasterGameAsync(new EditMasterGameRequest
            {
                MasterGameID = masterGameID,
                GameName = game.GameName,
                EstimatedReleaseDate = game.EstimatedReleaseDate,
                MinimumReleaseDate = game.MinimumReleaseDate,
                MaximumReleaseDate = game.MaximumReleaseDate,
                EarlyAccessReleaseDate = game.EarlyAccessReleaseDate,
                InternationalReleaseDate = game.InternationalReleaseDate,
                AnnouncementDate = game.AnnouncementDate,
                ReleaseDate = game.ReleaseDate,
                Tags = game.Tags.ToList(),
                SyncWithExternalAPIs = game.SyncWithExternalAPIs,
                UseSimpleEligibility = game.UseSimpleEligibility,
                DelayContention = game.DelayContention,
                ShowNote = game.ShowNote,
                OpenCriticID = game.OpenCriticID,
                GgToken = game.GgToken,
                Notes = game.Notes,
                CriticScore = null,
                ClearCriticScore = true,
            });
        }
        catch
        {
            // Best-effort: do not let TearDown failures mask test failures.
        }
    }

    private async Task<Guid> SelectBidTargetAsync(
        ICollection<PossibleMasterGameYearViewModel> available,
        HashSet<Guid> excludedIDs)
    {
        var candidates = available
            .Where(g => g.IsAvailable && !g.Taken && !g.IsReleased
                && !excludedIDs.Contains(g.MasterGame.MasterGameID))
            .Select(g => g.MasterGame.MasterGameID)
            .ToList();

        foreach (var id in candidates)
        {
            var liveGame = await _adminSession.Game.MasterGameAsync(id);
            if (liveGame.CriticScore == null)
                return id;
        }
        throw new InvalidOperationException(
            "No suitable bid target found — all candidates already have a live critic score.");
    }

    private async Task<Guid> SelectCounterPickTargetAsync(
        ICollection<PublisherGameViewModel> possibleCPs)
    {
        var candidates = possibleCPs
            .Where(g => g.MasterGame != null && !g.MasterGame.DelayContention && !g.MasterGame.IsReleased)
            .Select(g => g.MasterGame!.MasterGameID)
            .ToList();

        foreach (var id in candidates)
        {
            var liveGame = await _adminSession.Game.MasterGameAsync(id);
            if (liveGame.CriticScore == null)
                return id;
        }
        throw new InvalidOperationException(
            "No suitable counter-pick target found — all candidates already have a live critic score.");
    }

    private static async Task PlaceBidAsync(
        TestPublisher publisher,
        Guid masterGameID,
        int bidAmount,
        bool counterPick,
        Guid? conditionalDropPublisherGameID)
    {
        var result = await publisher.Session.League.MakePickupBidAsync(new PickupBidRequest
        {
            PublisherID = publisher.PublisherID,
            MasterGameID = masterGameID,
            CounterPick = counterPick,
            BidAmount = bidAmount,
            AllowIneligibleSlot = false,
            ConditionalDropPublisherGameID = conditionalDropPublisherGameID,
        });
        if (!result.Success)
        {
            var errors = string.Join("; ", result.Errors ?? []);
            throw new InvalidOperationException(
                $"MakePickupBid failed for publisher {publisher.PublisherID}, game {masterGameID}, " +
                $"amount {bidAmount}, counterPick {counterPick}. Errors: {errors}");
        }
    }

    private static async Task PlaceDropAsync(TestPublisher publisher, Guid publisherGameID)
    {
        var result = await publisher.Session.League.MakeDropRequestAsync(new DropGameRequestRequest
        {
            PublisherID = publisher.PublisherID,
            PublisherGameID = publisherGameID,
        });
        if (!result.Success)
        {
            var errors = string.Join("; ", result.Errors ?? []);
            throw new InvalidOperationException(
                $"MakeDropRequest failed for publisher {publisher.PublisherID}, " +
                $"publisherGame {publisherGameID}. Errors: {errors}");
        }
    }

    [Test]
    public void Bid_EligibleGame_Succeeds()
    {
        var p1 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[0].PublisherID);
        Assert.That(
            p1.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.GameA),
            Is.True,
            "P1 should have Game A after the uncontested bid succeeds.");
    }

    [Test]
    public void Bid_EligibleGame_BudgetDeducted()
    {
        var p1 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[0].PublisherID);
        Assert.That(p1.Budget, Is.EqualTo(_p1StartBudget - 10),
            "P1's budget should be reduced by $10.");
    }

    [Test]
    public void Bid_ScoredGame_Fails_NotOnRoster()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[1].PublisherID);
        Assert.That(
            p2.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.GameB),
            Is.False,
            "P2 should NOT have Game B — the game gained a score before processing.");
    }

    [Test]
    public void Bid_ScoredGame_Fails_BudgetUnchanged()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[1].PublisherID);
        Assert.That(p2.Budget, Is.EqualTo(_p2StartBudget),
            "P2's budget should be unchanged — all P2 bids failed.");
    }

    [Test]
    public void Drop_EligibleGame_Succeeds_NotOnRoster()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[2].PublisherID);
        Assert.That(
            p3.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.P3StandaloneDropMasterGameID),
            Is.False,
            "P3's standalone-dropped game should no longer be on the roster.");
    }

    [Test]
    public void Drop_EligibleGame_Succeeds_InFormerGames()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[2].PublisherID);
        Assert.That(
            p3.FormerGames.Any(g => g.MasterGame?.MasterGameID == _targets.P3StandaloneDropMasterGameID),
            Is.True,
            "P3's standalone-dropped game should appear in FormerGames.");
    }

    [Test]
    public void Drop_ScoredGame_Fails_StaysOnRoster()
    {
        var p4 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[3].PublisherID);
        Assert.That(
            p4.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.P4StandaloneDropMasterGameID),
            Is.True,
            "P4's drop target should still be on the roster — it gained a score before processing.");
    }

    [Test]
    public void ConditionalDrop_EligibleGame_Succeeds_BidGameOnRoster()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[2].PublisherID);
        Assert.That(
            p3.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.GameE),
            Is.True,
            "P3 should have Game E after the conditional-drop bid succeeds.");
    }

    [Test]
    public void ConditionalDrop_EligibleGame_Succeeds_DroppedGameGone()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[2].PublisherID);
        Assert.That(
            p3.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.P3ConditionalDropMasterGameID),
            Is.False,
            "P3's conditionally-dropped game should no longer be on the roster.");
    }

    [Test]
    public void ConditionalDrop_ScoredGame_Fails_BidGameNotOnRoster()
    {
        var p4 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[3].PublisherID);
        Assert.That(
            p4.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.GameG),
            Is.False,
            "P4 should NOT have Game G — both G and H (conditional drop) gained a score, causing the bid to fail.");
    }

    [Test]
    public void ConditionalDrop_ScoredGame_Fails_GameStaysOnRoster()
    {
        var p4 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[3].PublisherID);
        Assert.That(
            p4.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.P4ConditionalDropMasterGameID),
            Is.True,
            "P4's conditional-drop game (H) should still be on the roster — it gained a score so the drop was rejected.");
    }

    [Test]
    public void CounterPickBid_ScoredGame_Fails_NotOnRoster()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[1].PublisherID);
        Assert.That(
            p2.Games.Any(g => g.CounterPick && g.MasterGame?.MasterGameID == _targets.P2CounterPickTarget),
            Is.False,
            "P2 should NOT have the counter-pick — the target game gained a score before processing.");
    }

    [Test]
    public void History_Bid_EligibleGame_Successful()
    {
        var bid = _actionSet.Bids.Single(b =>
            !b.CounterPick && b.MasterGame.MasterGameID == _targets.GameA);
        Assert.That(bid.Successful, Is.True,
            "The action history should record Game A's bid as successful.");
    }

    [Test]
    public void History_Bid_ScoredGame_Unsuccessful_WithScoreOutcome()
    {
        var bid = _actionSet.Bids.Single(b =>
            !b.CounterPick && b.MasterGame.MasterGameID == _targets.GameB);
        Assert.That(bid.Successful, Is.False,
            "The action history should record Game B's bid as unsuccessful.");
        Assert.That(bid.Outcome, Does.Contain("score").IgnoreCase,
            "The failure outcome should mention 'score'.");
    }

    [Test]
    public void History_Drop_EligibleGame_Successful()
    {
        var drop = _actionSet.Drops.Single(d =>
            d.PublisherID == _league.Publishers[2].PublisherID
            && d.MasterGame.MasterGameID == _targets.P3StandaloneDropMasterGameID);
        Assert.That(drop.Successful, Is.True,
            "The action history should record P3's standalone drop as successful.");
    }

    [Test]
    public void History_Drop_ScoredGame_Unsuccessful()
    {
        var drop = _actionSet.Drops.Single(d =>
            d.PublisherID == _league.Publishers[3].PublisherID
            && d.MasterGame.MasterGameID == _targets.P4StandaloneDropMasterGameID);
        Assert.That(drop.Successful, Is.False,
            "The action history should record P4's drop as unsuccessful — the game gained a score.");
    }

    [Test]
    public void History_ConditionalDropBid_ScoredGame_Unsuccessful_WithScoreOutcome()
    {
        var bid = _actionSet.Bids.Single(b =>
            !b.CounterPick && b.MasterGame.MasterGameID == _targets.GameG);
        Assert.That(bid.Successful, Is.False,
            "The action history should record Game G's bid as unsuccessful — G gained a score before processing.");
        Assert.That(bid.Outcome, Does.Contain("score").IgnoreCase,
            "The failure outcome should mention 'score'.");
    }

    [Test]
    public void History_CounterPickBid_ScoredGame_Unsuccessful_WithScoreOutcome()
    {
        var bid = _actionSet.Bids.Single(b =>
            b.CounterPick && b.MasterGame.MasterGameID == _targets.P2CounterPickTarget);
        Assert.That(bid.Successful, Is.False,
            "The action history should record the counter-pick bid as unsuccessful.");
        Assert.That(bid.Outcome, Does.Contain("score").IgnoreCase,
            "The failure outcome should mention 'score'.");
    }
}
