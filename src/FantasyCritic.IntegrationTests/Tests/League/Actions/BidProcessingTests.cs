using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Actions;

/// <summary>
/// Tests the full post-draft pickup-bid lifecycle:
/// draft to completion → place bids (uncontested, contested, tiebreaker, counter-pick)
/// → advance clock past the Saturday 20:00 ET window → ProcessActions → assert results.
/// </summary>
[TestFixture]
public class BidProcessingTests : IntegrationTestBase
{
    private sealed record BidTargets(
        Guid GameA,
        Guid GameB,
        Guid GameC,
        Guid CounterPickTarget);

    private ApiSession _adminSession = null!;
    private LeagueFixture _league = null!;
    private BidTargets _targets = null!;

    private LeagueYearViewModel _postProcessingSnapshot = null!;
    private Guid _tiebreakerWinnerID;
    private Guid _tiebreakerLoserID;

    [OneTimeSetUp]
    public async Task SetUpBidProcessingLeague()
    {
        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);

        await _adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 6, 12, 0, 0, TimeSpan.Zero)
        });

        _league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, LeagueScenarios.FourPlayerBidding, NewUser);
        await _league.DraftToCompletionAsync();

        var postDraftSnapshot = await _league.GetLeagueYearAsync();
        _league.CapturePublisherState(postDraftSnapshot);

        var p1Available = await _league.Publishers[0].Session.League.TopAvailableGamesAsync(
            _league.Year, _league.LeagueID, _league.Publishers[0].PublisherID, null);
        var gameA = p1Available.First(g => g.IsAvailable && !g.Taken && !g.IsReleased);
        var gameAMasterGameID = gameA.MasterGame.MasterGameID;

        var p2Available = await _league.Publishers[1].Session.League.TopAvailableGamesAsync(
            _league.Year, _league.LeagueID, _league.Publishers[1].PublisherID, null);
        var gameB = p2Available.First(g => g.IsAvailable && !g.Taken && !g.IsReleased
            && g.MasterGame.MasterGameID != gameAMasterGameID);
        var gameBMasterGameID = gameB.MasterGame.MasterGameID;

        var p3Available = await _league.Publishers[2].Session.League.TopAvailableGamesAsync(
            _league.Year, _league.LeagueID, _league.Publishers[2].PublisherID, null);
        var gameC = p3Available.First(g => g.IsAvailable && !g.Taken && !g.IsReleased
            && g.MasterGame.MasterGameID != gameAMasterGameID
            && g.MasterGame.MasterGameID != gameBMasterGameID);
        var gameCMasterGameID = gameC.MasterGame.MasterGameID;

        var possibleCounterPicks = await _league.Publishers[1].Session.League.PossibleCounterPicksAsync(
            _league.Publishers[1].PublisherID);
        var counterPickTargetID = possibleCounterPicks
            .First(g => g.MasterGame != null && !g.MasterGame.DelayContention && !g.MasterGame.IsReleased)
            .MasterGame!.MasterGameID;

        _targets = new BidTargets(gameAMasterGameID, gameBMasterGameID, gameCMasterGameID, counterPickTargetID);

        await PlaceBidAsync(_league.Publishers[0], _targets.GameA, 10, false);
        await PlaceBidAsync(_league.Publishers[1], _targets.GameB, 20, false);
        await PlaceBidAsync(_league.Publishers[2], _targets.GameB, 10, false);
        await PlaceBidAsync(_league.Publishers[2], _targets.GameC, 15, false);
        await PlaceBidAsync(_league.Publishers[3], _targets.GameC, 15, false);
        await PlaceBidAsync(_league.Publishers[1], _targets.CounterPickTarget, 5, true);

        await _adminSession.Admin.SetTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 12, 1, 1, 0, TimeSpan.Zero)
        });

        await _adminSession.ActionRunner.TurnOnActionProcessingModeAsync();
        await _adminSession.ActionRunner.ProcessActionsAsync();
        await _adminSession.ActionRunner.TurnOffActionProcessingModeAsync();

        _postProcessingSnapshot = await _adminSession.League.GetLeagueYearAsync(_league.LeagueID, _league.Year, null);

        var p3Publisher = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[2].PublisherID);
        bool p3WonGameC = p3Publisher.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.GameC);
        _tiebreakerWinnerID = p3WonGameC ? _league.Publishers[2].PublisherID : _league.Publishers[3].PublisherID;
        _tiebreakerLoserID = p3WonGameC ? _league.Publishers[3].PublisherID : _league.Publishers[2].PublisherID;
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        if (_adminSession != null)
        {
            await _adminSession.ActionRunner.TurnOffActionProcessingModeAsync();
            await _adminSession.Admin.ResetTimeAsync();
        }
        _adminSession?.Dispose();
        if (_league != null)
            await _league.DisposeAsync();
    }

    private static async Task PlaceBidAsync(
        TestPublisher publisher,
        Guid masterGameID,
        int bidAmount,
        bool counterPick)
    {
        var result = await publisher.Session.League.MakePickupBidAsync(new PickupBidRequest
        {
            PublisherID = publisher.PublisherID,
            MasterGameID = masterGameID,
            CounterPick = counterPick,
            BidAmount = bidAmount,
            AllowIneligibleSlot = false,
            ConditionalDropPublisherGameID = null,
        });

        if (!result.Success)
        {
            var errors = string.Join("; ", result.Errors ?? []);
            throw new InvalidOperationException(
                $"MakePickupBid failed for publisher {publisher.PublisherID}, game {masterGameID}, " +
                $"amount {bidAmount}, counterPick {counterPick}. Errors: {errors}");
        }
    }

    [Test]
    public void Bid_Uncontested_P1_HasGameA_OnRoster()
    {
        var p1 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[0].PublisherID);
        Assert.That(p1.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.GameA), Is.True,
            "P1 should have Game A as a standard game after processing.");
    }

    [Test]
    public void Bid_Uncontested_P1_BudgetDeductedBy10()
    {
        var p1 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[0].PublisherID);
        Assert.That(p1.Budget, Is.EqualTo(_league.Publishers[0].StartingBudget - 10),
            "P1's budget should be reduced by $10 (uncontested bid amount).");
    }

    [Test]
    public void Bid_Contested_P2_HasGameB_OnRoster()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[1].PublisherID);
        Assert.That(p2.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.GameB), Is.True,
            "P2 should have Game B after winning the contested bid ($20 beats $10).");
    }

    [Test]
    public void Bid_Contested_P2_BudgetDeductedBy25()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[1].PublisherID);
        Assert.That(p2.Budget, Is.EqualTo(_league.Publishers[1].StartingBudget - 25),
            "P2's budget should be reduced by $25 ($20 for Game B + $5 for counter-pick).");
    }

    [Test]
    public void Bid_Contested_P3_DoesNotHaveGameB_OnRoster()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[2].PublisherID);
        Assert.That(p3.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.GameB), Is.False,
            "P3 should NOT have Game B after losing the contested bid.");
    }

    [Test]
    public void Bid_CounterPick_P2_HasP1Game_AsCounterPick()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[1].PublisherID);
        Assert.That(p2.Games.Any(g => g.CounterPick && g.MasterGame?.MasterGameID == _targets.CounterPickTarget), Is.True,
            "P2 should have P1's game as a counter-pick after the bid is processed.");
    }

    [Test]
    public void Bid_Tiebreaker_ExactlyOneOfP3P4_HasGameC()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[2].PublisherID);
        var p4 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[3].PublisherID);
        bool p3HasC = p3.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.GameC);
        bool p4HasC = p4.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.GameC);
        Assert.That(p3HasC ^ p4HasC, Is.True,
            "Exactly one of P3/P4 should have Game C after tiebreaker resolution.");
    }

    [Test]
    public void Bid_Tiebreaker_Winner_BudgetDeductedBy15()
    {
        var winnerBudgetStart = _tiebreakerWinnerID == _league.Publishers[2].PublisherID ? _league.Publishers[2].StartingBudget : _league.Publishers[3].StartingBudget;
        var winner = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _tiebreakerWinnerID);
        Assert.That(winner.Budget, Is.EqualTo(winnerBudgetStart - 15),
            "Tiebreaker winner's budget should be reduced by $15.");
    }

    [Test]
    public void Bid_Tiebreaker_Loser_BudgetUnchanged()
    {
        var loserBudgetStart = _tiebreakerLoserID == _league.Publishers[2].PublisherID ? _league.Publishers[2].StartingBudget : _league.Publishers[3].StartingBudget;
        var loser = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _tiebreakerLoserID);
        Assert.That(loser.Budget, Is.EqualTo(loserBudgetStart),
            "Tiebreaker loser's budget should be unchanged (losing bids are not charged).");
    }
}
