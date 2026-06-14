using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Actions;

/// <summary>
/// Tests drop and conditional-drop processing after a full draft:
/// standalone drop → drop + bid (drop frees slot before pickup) → conditional-drop bid.
/// </summary>
[TestFixture]
public class DropProcessingTests : IntegrationTestBase
{
    private sealed record DropTargets(
        Guid P1DropMasterGameID,
        Guid P1DropPublisherGameID,
        Guid P2DropMasterGameID,
        Guid P2DropPublisherGameID,
        Guid P2BidTargetMasterGameID,
        Guid P3ConditionalDropMasterGameID,
        Guid P3ConditionalDropPublisherGameID,
        Guid P3BidTargetMasterGameID);

    private ApiSession _adminSession = null!;
    private LeagueFixture _league = null!;
    private DropTargets _targets = null!;

    private LeagueYearViewModel _postProcessingSnapshot = null!;

    [OneTimeSetUp]
    public async Task SetUpDropProcessingLeague()
    {
        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);

        await _adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 6, 12, 0, 0, TimeSpan.Zero)
        });

        _league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, LeagueScenarios.FourPlayerDrops, NewUser);
        await _league.DraftToCompletionAsync();

        var postDraftSnapshot = await _league.GetLeagueYearAsync();
        var p1 = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[0].PublisherID);
        var p2 = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[1].PublisherID);
        var p3 = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[2].PublisherID);

        _league.Publishers[0].StartingBudget = p1.Budget;
        _league.Publishers[1].StartingBudget = p2.Budget;
        _league.Publishers[2].StartingBudget = p3.Budget;
        _league.Publishers[0].WillReleaseDroppableBefore = p1.WillReleaseGamesDropped;
        _league.Publishers[1].WillReleaseDroppableBefore = p2.WillReleaseGamesDropped;
        _league.Publishers[2].WillReleaseDroppableBefore = p3.WillReleaseGamesDropped;

        var p1DropGame = FindDroppableDraftedGame(p1);
        var p2DropGame = FindDroppableDraftedGame(p2);
        var p3DropGame = FindDroppableDraftedGame(p3);

        var usedMasterGameIDs = new[]
        {
            p1DropGame.MasterGame!.MasterGameID,
            p2DropGame.MasterGame!.MasterGameID,
            p3DropGame.MasterGame!.MasterGameID,
        };

        var p2BidTargetMasterGameID = await PickAvailableBidTargetAsync(
            _league.Publishers[1], usedMasterGameIDs);
        var p3BidTargetMasterGameID = await PickAvailableBidTargetAsync(
            _league.Publishers[2], usedMasterGameIDs.Append(p2BidTargetMasterGameID));

        _targets = new DropTargets(
            p1DropGame.MasterGame!.MasterGameID,
            p1DropGame.PublisherGameID,
            p2DropGame.MasterGame!.MasterGameID,
            p2DropGame.PublisherGameID,
            p2BidTargetMasterGameID,
            p3DropGame.MasterGame!.MasterGameID,
            p3DropGame.PublisherGameID,
            p3BidTargetMasterGameID);

        await PlaceDropAsync(_league.Publishers[0], _targets.P1DropPublisherGameID);
        await PlaceDropAsync(_league.Publishers[1], _targets.P2DropPublisherGameID);
        await PlaceBidAsync(_league.Publishers[1], _targets.P2BidTargetMasterGameID, 10, false, null);
        await PlaceBidAsync(_league.Publishers[2], _targets.P3BidTargetMasterGameID, 15, false, _targets.P3ConditionalDropPublisherGameID);

        await _adminSession.Admin.SetTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 12, 1, 1, 0, TimeSpan.Zero)
        });

        await _adminSession.ActionRunner.TurnOnActionProcessingModeAsync();
        await _adminSession.ActionRunner.ProcessActionsAsync();
        await _adminSession.ActionRunner.TurnOffActionProcessingModeAsync();

        _postProcessingSnapshot = await _adminSession.League.GetLeagueYearAsync(_league.LeagueID, _league.Year, null);
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

    private static PublisherGameViewModel FindDroppableDraftedGame(PublisherViewModel publisher)
    {
        return publisher.Games.First(g =>
            !g.CounterPick
            && !g.DropBlocked
            && g.MasterGame != null
            && g.OverallDraftPosition.HasValue);
    }

    private async Task<Guid> PickAvailableBidTargetAsync(
        TestPublisher publisher,
        IEnumerable<Guid> excludedMasterGameIDs)
    {
        var excluded = excludedMasterGameIDs.ToHashSet();
        var available = await publisher.Session.League.TopAvailableGamesAsync(
            _league.Year, _league.LeagueID, publisher.PublisherID, null);
        var target = available.First(g =>
            g.IsAvailable
            && !g.Taken
            && !g.IsReleased
            && g.MasterGame != null
            && !excluded.Contains(g.MasterGame.MasterGameID));

        return target.MasterGame!.MasterGameID;
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
                $"MakeDropRequest failed for publisher {publisher.PublisherID}, publisherGame {publisherGameID}. Errors: {errors}");
        }
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
                $"amount {bidAmount}, counterPick {counterPick}, conditionalDrop {conditionalDropPublisherGameID}. " +
                $"Errors: {errors}");
        }
    }

    [Test]
    public void Drop_Standalone_P1_DroppedGame_NotOnRoster()
    {
        var p1 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[0].PublisherID);
        Assert.That(
            p1.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.P1DropMasterGameID),
            Is.False,
            "P1's standalone-dropped game should no longer be on the roster.");
    }

    [Test]
    public void Drop_Standalone_P1_DroppedGame_InFormerGames()
    {
        var p1 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[0].PublisherID);
        Assert.That(
            p1.FormerGames.Any(g => g.MasterGame?.MasterGameID == _targets.P1DropMasterGameID),
            Is.True,
            "P1's standalone-dropped game should appear in FormerGames.");
    }

    [Test]
    public void Drop_Standalone_P1_WillReleaseDroppedCountIncremented()
    {
        var p1 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[0].PublisherID);
        Assert.That(
            p1.WillReleaseGamesDropped,
            Is.EqualTo(_league.Publishers[0].WillReleaseDroppableBefore + 1),
            "P1 should consume one will-release drop allowance.");
    }

    [Test]
    public void DropAndBid_P2_DroppedGame_NotOnRoster()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[1].PublisherID);
        Assert.That(
            p2.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.P2DropMasterGameID),
            Is.False,
            "P2's explicitly dropped game should no longer be on the roster.");
    }

    [Test]
    public void DropAndBid_P2_BidTarget_OnRoster()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[1].PublisherID);
        Assert.That(
            p2.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.P2BidTargetMasterGameID),
            Is.True,
            "P2 should acquire the bid target after the drop frees a roster slot.");
    }

    [Test]
    public void DropAndBid_P2_BudgetDeductedBy10()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[1].PublisherID);
        Assert.That(p2.Budget, Is.EqualTo(_league.Publishers[1].StartingBudget - 10),
            "P2's budget should be reduced by the uncontested bid amount.");
    }

    [Test]
    public void ConditionalDrop_P3_DroppedGame_NotOnRoster()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[2].PublisherID);
        Assert.That(
            p3.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.P3ConditionalDropMasterGameID),
            Is.False,
            "P3's conditionally dropped game should no longer be on the roster.");
    }

    [Test]
    public void ConditionalDrop_P3_DroppedGame_InFormerGames()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[2].PublisherID);
        Assert.That(
            p3.FormerGames.Any(g => g.MasterGame?.MasterGameID == _targets.P3ConditionalDropMasterGameID),
            Is.True,
            "P3's conditionally dropped game should appear in FormerGames.");
    }

    [Test]
    public void ConditionalDrop_P3_BidTarget_OnRoster()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[2].PublisherID);
        Assert.That(
            p3.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _targets.P3BidTargetMasterGameID),
            Is.True,
            "P3 should acquire the bid target when the conditional drop succeeds.");
    }

    [Test]
    public void ConditionalDrop_P3_BudgetDeductedBy15()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[2].PublisherID);
        Assert.That(p3.Budget, Is.EqualTo(_league.Publishers[2].StartingBudget - 15),
            "P3's budget should be reduced by the conditional-drop bid amount.");
    }

    [Test]
    public void ConditionalDrop_P3_WillReleaseDroppedCountIncremented()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[2].PublisherID);
        Assert.That(
            p3.WillReleaseGamesDropped,
            Is.EqualTo(_league.Publishers[2].WillReleaseDroppableBefore + 1),
            "P3 should consume one will-release drop allowance via conditional drop.");
    }

    [Test]
    public void DropAndBid_P2_WillReleaseDroppedCountIncremented()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[1].PublisherID);
        Assert.That(
            p2.WillReleaseGamesDropped,
            Is.EqualTo(_league.Publishers[1].WillReleaseDroppableBefore + 1),
            "P2 should consume one will-release drop allowance for the explicit drop.");
    }
}
