using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League;

/// <summary>
/// Tests drop and conditional-drop processing after a full draft:
/// standalone drop → drop + bid (drop frees slot before pickup) → conditional-drop bid.
/// </summary>
[TestFixture]
public class DropProcessingTests : IntegrationTestBase
{
    private ApiSession _adminSession = null!;
    private ApiSession _managerSession = null!;
    private ApiSession _p2Session = null!;
    private ApiSession _p3Session = null!;
    private ApiSession _p4Session = null!;

    private Guid _leagueID;
    private int _year;

    private Guid _p1PublisherID;
    private Guid _p2PublisherID;
    private Guid _p3PublisherID;
    private Guid _p4PublisherID;

    private int _p1StartBudget;
    private int _p2StartBudget;
    private int _p3StartBudget;

    private Guid _p1DropTargetMasterGameID;
    private Guid _p1DropTargetPublisherGameID;

    private Guid _p2DropTargetMasterGameID;
    private Guid _p2DropTargetPublisherGameID;
    private Guid _p2BidTargetMasterGameID;

    private Guid _p3ConditionalDropTargetMasterGameID;
    private Guid _p3ConditionalDropPublisherGameID;
    private Guid _p3BidTargetMasterGameID;

    private int _p1WillReleaseDroppedBefore;
    private int _p2WillReleaseDroppedBefore;
    private int _p3WillReleaseDroppedBefore;

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

        var (mgrEmail, mgrPassword, mgrDisplayName) = NewUser();
        _managerSession = new ApiSession(Factory);
        await _managerSession.RegisterAsync(mgrEmail, mgrPassword, mgrDisplayName);

        var (p2Email, p2Password, p2DisplayName) = NewUser();
        _p2Session = new ApiSession(Factory);
        await _p2Session.RegisterAsync(p2Email, p2Password, p2DisplayName);

        var (p3Email, p3Password, p3DisplayName) = NewUser();
        _p3Session = new ApiSession(Factory);
        await _p3Session.RegisterAsync(p3Email, p3Password, p3DisplayName);

        var (p4Email, p4Password, p4DisplayName) = NewUser();
        _p4Session = new ApiSession(Factory);
        await _p4Session.RegisterAsync(p4Email, p4Password, p4DisplayName);

        _year = await LeagueTestHelpers.GetOpenYearAsync(_managerSession);
        _leagueID = await LeagueTestHelpers.CreateLeagueAsync(_managerSession, LeagueScenarios.FourPlayerDrops, _year);

        await LeagueTestHelpers.InviteAndAcceptAsync(_managerSession, _p2Session, _leagueID);
        await LeagueTestHelpers.InviteAndAcceptAsync(_managerSession, _p3Session, _leagueID);
        await LeagueTestHelpers.InviteAndAcceptAsync(_managerSession, _p4Session, _leagueID);

        _p1PublisherID = await LeagueTestHelpers.CreatePublisherAsync(_managerSession, _leagueID, _year, "P1 Publisher");
        _p2PublisherID = await LeagueTestHelpers.CreatePublisherAsync(_p2Session, _leagueID, _year, "P2 Publisher");
        _p3PublisherID = await LeagueTestHelpers.CreatePublisherAsync(_p3Session, _leagueID, _year, "P3 Publisher");
        _p4PublisherID = await LeagueTestHelpers.CreatePublisherAsync(_p4Session, _leagueID, _year, "P4 Publisher");

        await LeagueTestHelpers.SetDraftOrderAsync(_managerSession, _leagueID, _year,
            [_p1PublisherID, _p2PublisherID, _p3PublisherID, _p4PublisherID]);

        await _managerSession.LeagueManager.StartDraftAsync(new StartDraftRequest
        {
            LeagueID = _leagueID,
            Year = _year,
        });

        var players = new[]
        {
            new MockedLivePlayer(_managerSession, _p1PublisherID, _leagueID),
            new MockedLivePlayer(_p2Session, _p2PublisherID, _leagueID),
            new MockedLivePlayer(_p3Session, _p3PublisherID, _leagueID),
            new MockedLivePlayer(_p4Session, _p4PublisherID, _leagueID),
        };
        var simulator = new DraftSimulator(_managerSession, players);
        await simulator.RunAsync(_leagueID, _year);

        var postDraftSnapshot = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);
        var p1 = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID);
        var p2 = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _p2PublisherID);
        var p3 = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID);

        _p1StartBudget = p1.Budget;
        _p2StartBudget = p2.Budget;
        _p3StartBudget = p3.Budget;

        _p1WillReleaseDroppedBefore = p1.WillReleaseGamesDropped;
        _p2WillReleaseDroppedBefore = p2.WillReleaseGamesDropped;
        _p3WillReleaseDroppedBefore = p3.WillReleaseGamesDropped;

        var p1DropGame = FindDroppableDraftedGame(p1);
        _p1DropTargetPublisherGameID = p1DropGame.PublisherGameID;
        _p1DropTargetMasterGameID = p1DropGame.MasterGame!.MasterGameID;

        var p2DropGame = FindDroppableDraftedGame(p2);
        _p2DropTargetPublisherGameID = p2DropGame.PublisherGameID;
        _p2DropTargetMasterGameID = p2DropGame.MasterGame!.MasterGameID;

        var p3DropGame = FindDroppableDraftedGame(p3);
        _p3ConditionalDropPublisherGameID = p3DropGame.PublisherGameID;
        _p3ConditionalDropTargetMasterGameID = p3DropGame.MasterGame!.MasterGameID;

        var usedMasterGameIDs = new[]
        {
            _p1DropTargetMasterGameID,
            _p2DropTargetMasterGameID,
            _p3ConditionalDropTargetMasterGameID,
        };

        _p2BidTargetMasterGameID = await PickAvailableBidTargetAsync(
            _p2Session, _p2PublisherID, usedMasterGameIDs);
        _p3BidTargetMasterGameID = await PickAvailableBidTargetAsync(
            _p3Session, _p3PublisherID, usedMasterGameIDs.Append(_p2BidTargetMasterGameID));

        await PlaceDropAsync(_managerSession, _p1PublisherID, _p1DropTargetPublisherGameID);
        await PlaceDropAsync(_p2Session, _p2PublisherID, _p2DropTargetPublisherGameID);
        await PlaceBidAsync(_p2Session, _p2PublisherID, _p2BidTargetMasterGameID, 10, false, null);
        await PlaceBidAsync(_p3Session, _p3PublisherID, _p3BidTargetMasterGameID, 15, false, _p3ConditionalDropPublisherGameID);

        await _adminSession.Admin.SetTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 12, 1, 1, 0, TimeSpan.Zero)
        });

        await _adminSession.ActionRunner.TurnOnActionProcessingModeAsync();
        await _adminSession.ActionRunner.ProcessActionsAsync();
        await _adminSession.ActionRunner.TurnOffActionProcessingModeAsync();

        _postProcessingSnapshot = await _adminSession.League.GetLeagueYearAsync(_leagueID, _year, null);
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
        _managerSession?.Dispose();
        _p2Session?.Dispose();
        _p3Session?.Dispose();
        _p4Session?.Dispose();
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
        ApiSession session,
        Guid publisherID,
        IEnumerable<Guid> excludedMasterGameIDs)
    {
        var excluded = excludedMasterGameIDs.ToHashSet();
        var available = await session.League.TopAvailableGamesAsync(_year, _leagueID, publisherID, null);
        var target = available.First(g =>
            g.IsAvailable
            && !g.Taken
            && !g.IsReleased
            && g.MasterGame != null
            && !excluded.Contains(g.MasterGame.MasterGameID));

        return target.MasterGame!.MasterGameID;
    }

    private static async Task PlaceDropAsync(ApiSession session, Guid publisherID, Guid publisherGameID)
    {
        var result = await session.League.MakeDropRequestAsync(new DropGameRequestRequest
        {
            PublisherID = publisherID,
            PublisherGameID = publisherGameID,
        });

        if (!result.Success)
        {
            var errors = string.Join("; ", result.Errors ?? []);
            throw new InvalidOperationException(
                $"MakeDropRequest failed for publisher {publisherID}, publisherGame {publisherGameID}. Errors: {errors}");
        }
    }

    private static async Task PlaceBidAsync(
        ApiSession session,
        Guid publisherID,
        Guid masterGameID,
        int bidAmount,
        bool counterPick,
        Guid? conditionalDropPublisherGameID)
    {
        var result = await session.League.MakePickupBidAsync(new PickupBidRequest
        {
            PublisherID = publisherID,
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
                $"MakePickupBid failed for publisher {publisherID}, game {masterGameID}, " +
                $"amount {bidAmount}, counterPick {counterPick}, conditionalDrop {conditionalDropPublisherGameID}. " +
                $"Errors: {errors}");
        }
    }

    [Test]
    public void Drop_Standalone_P1_DroppedGame_NotOnRoster()
    {
        var p1 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID);
        Assert.That(
            p1.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _p1DropTargetMasterGameID),
            Is.False,
            "P1's standalone-dropped game should no longer be on the roster.");
    }

    [Test]
    public void Drop_Standalone_P1_DroppedGame_InFormerGames()
    {
        var p1 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID);
        Assert.That(
            p1.FormerGames.Any(g => g.MasterGame?.MasterGameID == _p1DropTargetMasterGameID),
            Is.True,
            "P1's standalone-dropped game should appear in FormerGames.");
    }

    [Test]
    public void Drop_Standalone_P1_WillReleaseDroppedCountIncremented()
    {
        var p1 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID);
        Assert.That(
            p1.WillReleaseGamesDropped,
            Is.EqualTo(_p1WillReleaseDroppedBefore + 1),
            "P1 should consume one will-release drop allowance.");
    }

    [Test]
    public void DropAndBid_P2_DroppedGame_NotOnRoster()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p2PublisherID);
        Assert.That(
            p2.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _p2DropTargetMasterGameID),
            Is.False,
            "P2's explicitly dropped game should no longer be on the roster.");
    }

    [Test]
    public void DropAndBid_P2_BidTarget_OnRoster()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p2PublisherID);
        Assert.That(
            p2.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _p2BidTargetMasterGameID),
            Is.True,
            "P2 should acquire the bid target after the drop frees a roster slot.");
    }

    [Test]
    public void DropAndBid_P2_BudgetDeductedBy10()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p2PublisherID);
        Assert.That(p2.Budget, Is.EqualTo(_p2StartBudget - 10),
            "P2's budget should be reduced by the uncontested bid amount.");
    }

    [Test]
    public void ConditionalDrop_P3_DroppedGame_NotOnRoster()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID);
        Assert.That(
            p3.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _p3ConditionalDropTargetMasterGameID),
            Is.False,
            "P3's conditionally dropped game should no longer be on the roster.");
    }

    [Test]
    public void ConditionalDrop_P3_DroppedGame_InFormerGames()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID);
        Assert.That(
            p3.FormerGames.Any(g => g.MasterGame?.MasterGameID == _p3ConditionalDropTargetMasterGameID),
            Is.True,
            "P3's conditionally dropped game should appear in FormerGames.");
    }

    [Test]
    public void ConditionalDrop_P3_BidTarget_OnRoster()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID);
        Assert.That(
            p3.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _p3BidTargetMasterGameID),
            Is.True,
            "P3 should acquire the bid target when the conditional drop succeeds.");
    }

    [Test]
    public void ConditionalDrop_P3_BudgetDeductedBy15()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID);
        Assert.That(p3.Budget, Is.EqualTo(_p3StartBudget - 15),
            "P3's budget should be reduced by the conditional-drop bid amount.");
    }

    [Test]
    public void ConditionalDrop_P3_WillReleaseDroppedCountIncremented()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID);
        Assert.That(
            p3.WillReleaseGamesDropped,
            Is.EqualTo(_p3WillReleaseDroppedBefore + 1),
            "P3 should consume one will-release drop allowance via conditional drop.");
    }

    [Test]
    public void DropAndBid_P2_WillReleaseDroppedCountIncremented()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p2PublisherID);
        Assert.That(
            p2.WillReleaseGamesDropped,
            Is.EqualTo(_p2WillReleaseDroppedBefore + 1),
            "P2 should consume one will-release drop allowance for the explicit drop.");
    }
}
