using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League;

/// <summary>
/// Tests the full post-draft pickup-bid lifecycle:
/// draft to completion → place bids (uncontested, contested, tiebreaker, counter-pick)
/// → advance clock past the Saturday 20:00 ET window → ProcessActions → assert results.
/// </summary>
[TestFixture]
public class BidProcessingTests : IntegrationTestBase
{
    // Sessions
    private ApiSession _adminSession = null!;
    private ApiSession _managerSession = null!;
    private ApiSession _p2Session = null!;
    private ApiSession _p3Session = null!;
    private ApiSession _p4Session = null!;

    // League
    private Guid _leagueID;
    private int _year;

    // Publisher IDs (in draft order: P1=manager, P2, P3, P4)
    private Guid _p1PublisherID;
    private Guid _p2PublisherID;
    private Guid _p3PublisherID;
    private Guid _p4PublisherID;

    // Post-draft budgets (all publishers start at $100)
    private int _p1StartBudget;
    private int _p2StartBudget;
    private int _p3StartBudget;
    private int _p4StartBudget;

    // Bid targets
    private Guid _gameAMasterGameID;
    private Guid _gameBMasterGameID;
    private Guid _gameCMasterGameID;
    private Guid _p1CounterPickTargetID;

    // Post-processing state
    private LeagueYearViewModel _postProcessingSnapshot = null!;
    private Guid _tiebreakerWinnerID;
    private Guid _tiebreakerLoserID;

    [OneTimeSetUp]
    public async Task SetUpBidProcessingLeague()
    {
        // ── Phase 1: Clock and draft ───────────────────────────────────────────

        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);

        // Set clock to Monday Jan 6, 2025 12:00 UTC — safely outside the bidding window
        await _adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 6, 12, 0, 0, TimeSpan.Zero)
        });

        // Register P1 (manager)
        var (mgrEmail, mgrPassword, mgrDisplayName) = NewUser();
        _managerSession = new ApiSession(Factory);
        await _managerSession.RegisterAsync(mgrEmail, mgrPassword, mgrDisplayName);

        // Register P2, P3, P4
        var (p2Email, p2Password, p2DisplayName) = NewUser();
        _p2Session = new ApiSession(Factory);
        await _p2Session.RegisterAsync(p2Email, p2Password, p2DisplayName);

        var (p3Email, p3Password, p3DisplayName) = NewUser();
        _p3Session = new ApiSession(Factory);
        await _p3Session.RegisterAsync(p3Email, p3Password, p3DisplayName);

        var (p4Email, p4Password, p4DisplayName) = NewUser();
        _p4Session = new ApiSession(Factory);
        await _p4Session.RegisterAsync(p4Email, p4Password, p4DisplayName);

        // Create FourPlayerBidding league under manager
        _year = await LeagueTestHelpers.GetOpenYearAsync(_managerSession);
        _leagueID = await LeagueTestHelpers.CreateLeagueAsync(_managerSession, LeagueScenarios.FourPlayerBidding, _year);

        // Invite P2, P3, P4 and have them accept
        await LeagueTestHelpers.InviteAndAcceptAsync(_managerSession, _p2Session, _leagueID);
        await LeagueTestHelpers.InviteAndAcceptAsync(_managerSession, _p3Session, _leagueID);
        await LeagueTestHelpers.InviteAndAcceptAsync(_managerSession, _p4Session, _leagueID);

        // Create publishers
        _p1PublisherID = await LeagueTestHelpers.CreatePublisherAsync(_managerSession, _leagueID, _year, "P1 Publisher");
        _p2PublisherID = await LeagueTestHelpers.CreatePublisherAsync(_p2Session, _leagueID, _year, "P2 Publisher");
        _p3PublisherID = await LeagueTestHelpers.CreatePublisherAsync(_p3Session, _leagueID, _year, "P3 Publisher");
        _p4PublisherID = await LeagueTestHelpers.CreatePublisherAsync(_p4Session, _leagueID, _year, "P4 Publisher");

        // Set draft order: P1=1, P2=2, P3=3, P4=4
        await LeagueTestHelpers.SetDraftOrderAsync(_managerSession, _leagueID, _year,
            [_p1PublisherID, _p2PublisherID, _p3PublisherID, _p4PublisherID]);

        // Start draft
        await _managerSession.LeagueManager.StartDraftAsync(new StartDraftRequest
        {
            LeagueID = _leagueID,
            Year = _year,
        });

        // Simulate full draft
        var players = new[]
        {
            new MockedLivePlayer(_managerSession, _p1PublisherID, _leagueID),
            new MockedLivePlayer(_p2Session, _p2PublisherID, _leagueID),
            new MockedLivePlayer(_p3Session, _p3PublisherID, _leagueID),
            new MockedLivePlayer(_p4Session, _p4PublisherID, _leagueID),
        };
        var simulator = new DraftSimulator(_managerSession, players);
        await simulator.RunAsync(_leagueID, _year);

        // Capture post-draft budgets
        var postDraftSnapshot = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);
        _p1StartBudget = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID).Budget;
        _p2StartBudget = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _p2PublisherID).Budget;
        _p3StartBudget = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID).Budget;
        _p4StartBudget = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _p4PublisherID).Budget;

        // ── Phase 2: Select game targets ──────────────────────────────────────

        // Game A — only P1 bids (uncontested)
        // Exclude already-released games: bids on them are rejected at processing time.
        var p1Available = await _managerSession.League.TopAvailableGamesAsync(_year, _leagueID, _p1PublisherID, null);
        var gameA = p1Available.First(g => g.IsAvailable && !g.Taken && !g.IsReleased);
        _gameAMasterGameID = gameA.MasterGame.MasterGameID;

        // Game B — P2 bids $20, P3 bids $10 (contested, P2 wins)
        var p2Available = await _p2Session.League.TopAvailableGamesAsync(_year, _leagueID, _p2PublisherID, null);
        var gameB = p2Available.First(g => g.IsAvailable && !g.Taken && !g.IsReleased
            && g.MasterGame.MasterGameID != _gameAMasterGameID);
        _gameBMasterGameID = gameB.MasterGame.MasterGameID;

        // Game C — P3 and P4 each bid $15 (tiebreaker)
        var p3Available = await _p3Session.League.TopAvailableGamesAsync(_year, _leagueID, _p3PublisherID, null);
        var gameC = p3Available.First(g => g.IsAvailable && !g.Taken && !g.IsReleased
            && g.MasterGame.MasterGameID != _gameAMasterGameID
            && g.MasterGame.MasterGameID != _gameBMasterGameID);
        _gameCMasterGameID = gameC.MasterGame.MasterGameID;

        // Counter-pick target: ask the server for games P2 can currently counter-pick, then pick one
        // that is not in delay contention (delay-contention games cannot be counter-picked via a bid,
        // only via the draft) and is not already released (released games cannot be picked up).
        var possibleCounterPicks = await _p2Session.League.PossibleCounterPicksAsync(_p2PublisherID);
        _p1CounterPickTargetID = possibleCounterPicks
            .First(g => g.MasterGame != null && !g.MasterGame.DelayContention && !g.MasterGame.IsReleased)
            .MasterGame!.MasterGameID;

        // ── Phase 3: Place six bids ────────────────────────────────────────────

        await PlaceBidAsync(_managerSession, _p1PublisherID, _gameAMasterGameID, 10, false);
        await PlaceBidAsync(_p2Session, _p2PublisherID, _gameBMasterGameID, 20, false);
        await PlaceBidAsync(_p3Session, _p3PublisherID, _gameBMasterGameID, 10, false);
        await PlaceBidAsync(_p3Session, _p3PublisherID, _gameCMasterGameID, 15, false);
        await PlaceBidAsync(_p4Session, _p4PublisherID, _gameCMasterGameID, 15, false);
        await PlaceBidAsync(_p2Session, _p2PublisherID, _p1CounterPickTargetID, 5, true);

        // ── Phase 4: Advance time and run bids ────────────────────────────────

        // Saturday Jan 11, 2025 20:01 ET = Jan 12, 2025 01:01 UTC (ET is UTC-5 in January)
        await _adminSession.Admin.SetTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 12, 1, 1, 0, TimeSpan.Zero)
        });

        await _adminSession.ActionRunner.TurnOnActionProcessingModeAsync();
        await _adminSession.ActionRunner.ProcessActionsAsync();
        await _adminSession.ActionRunner.TurnOffActionProcessingModeAsync();

        // ── Phase 5: Capture post-processing state ────────────────────────────

        _postProcessingSnapshot = await _adminSession.League.GetLeagueYearAsync(_leagueID, _year, null);

        // Resolve tiebreaker winner/loser dynamically
        var p3Publisher = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID);
        bool p3WonGameC = p3Publisher.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _gameCMasterGameID);
        _tiebreakerWinnerID = p3WonGameC ? _p3PublisherID : _p4PublisherID;
        _tiebreakerLoserID  = p3WonGameC ? _p4PublisherID : _p3PublisherID;
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

    // ── Helper ────────────────────────────────────────────────────────────────

    private static async Task PlaceBidAsync(
        ApiSession session,
        Guid publisherID,
        Guid masterGameID,
        int bidAmount,
        bool counterPick)
    {
        var result = await session.League.MakePickupBidAsync(new PickupBidRequest
        {
            PublisherID = publisherID,
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
                $"MakePickupBid failed for publisher {publisherID}, game {masterGameID}, " +
                $"amount {bidAmount}, counterPick {counterPick}. Errors: {errors}");
        }
    }

    // ── Test Methods ──────────────────────────────────────────────────────────

    [Test]
    public void Bid_Uncontested_P1_HasGameA_OnRoster()
    {
        var p1 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID);
        Assert.That(p1.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _gameAMasterGameID), Is.True,
            "P1 should have Game A as a standard game after processing.");
    }

    [Test]
    public void Bid_Uncontested_P1_BudgetDeductedBy10()
    {
        var p1 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID);
        Assert.That(p1.Budget, Is.EqualTo(_p1StartBudget - 10),
            "P1's budget should be reduced by $10 (uncontested bid amount).");
    }

    [Test]
    public void Bid_Contested_P2_HasGameB_OnRoster()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p2PublisherID);
        Assert.That(p2.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _gameBMasterGameID), Is.True,
            "P2 should have Game B after winning the contested bid ($20 beats $10).");
    }

    [Test]
    public void Bid_Contested_P2_BudgetDeductedBy25()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p2PublisherID);
        Assert.That(p2.Budget, Is.EqualTo(_p2StartBudget - 25),
            "P2's budget should be reduced by $25 ($20 for Game B + $5 for counter-pick).");
    }

    [Test]
    public void Bid_Contested_P3_DoesNotHaveGameB_OnRoster()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID);
        Assert.That(p3.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _gameBMasterGameID), Is.False,
            "P3 should NOT have Game B after losing the contested bid.");
    }

    [Test]
    public void Bid_CounterPick_P2_HasP1Game_AsCounterPick()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p2PublisherID);
        Assert.That(p2.Games.Any(g => g.CounterPick && g.MasterGame?.MasterGameID == _p1CounterPickTargetID), Is.True,
            "P2 should have P1's game as a counter-pick after the bid is processed.");
    }

    [Test]
    public void Bid_Tiebreaker_ExactlyOneOfP3P4_HasGameC()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID);
        var p4 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p4PublisherID);
        bool p3HasC = p3.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _gameCMasterGameID);
        bool p4HasC = p4.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _gameCMasterGameID);
        Assert.That(p3HasC ^ p4HasC, Is.True,
            "Exactly one of P3/P4 should have Game C after tiebreaker resolution.");
    }

    [Test]
    public void Bid_Tiebreaker_Winner_BudgetDeductedBy15()
    {
        var winnerBudgetStart = _tiebreakerWinnerID == _p3PublisherID ? _p3StartBudget : _p4StartBudget;
        var winner = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _tiebreakerWinnerID);
        Assert.That(winner.Budget, Is.EqualTo(winnerBudgetStart - 15),
            "Tiebreaker winner's budget should be reduced by $15.");
    }

    [Test]
    public void Bid_Tiebreaker_Loser_BudgetUnchanged()
    {
        var loserBudgetStart = _tiebreakerLoserID == _p3PublisherID ? _p3StartBudget : _p4StartBudget;
        var loser = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _tiebreakerLoserID);
        Assert.That(loser.Budget, Is.EqualTo(loserBudgetStart),
            "Tiebreaker loser's budget should be unchanged (losing bids are not charged).");
    }
}
