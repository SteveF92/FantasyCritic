using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League;

/// <summary>
/// Tests that bids and drops which were eligible when placed are correctly
/// rejected at processing time when the target (or conditionally-dropped) game
/// gains a critic score in the intervening window.
///
/// Eligibility rule (GameEligibilityFunctions.GetGenericSlotMasterGameErrors):
///   hasScore → "That game already has a score." for both bids and drops.
/// </summary>
[TestFixture]
public class EligibilityChangeTests : IntegrationTestBase
{
    // ── Sessions ──────────────────────────────────────────────────────────────
    private ApiSession _adminSession = null!;
    private ApiSession _managerSession = null!;
    private ApiSession _p2Session = null!;
    private ApiSession _p3Session = null!;
    private ApiSession _p4Session = null!;

    // ── League ────────────────────────────────────────────────────────────────
    private Guid _leagueID;
    private int _year;

    // ── Publisher IDs ─────────────────────────────────────────────────────────
    private Guid _p1PublisherID;
    private Guid _p2PublisherID;
    private Guid _p3PublisherID;
    private Guid _p4PublisherID;

    // ── Post-draft budgets ────────────────────────────────────────────────────
    private int _p1StartBudget;
    private int _p2StartBudget;

    // ── Bid targets ───────────────────────────────────────────────────────────
    private Guid _gameAMasterGameID;                   // P1 bids; stays clean  → bid succeeds
    private Guid _gameBMasterGameID;                   // P2 bids; gets score   → bid fails
    private Guid _gameEMasterGameID;                   // P3 conditional-drop bids; stays clean → bid succeeds
    private Guid _gameGMasterGameID;                   // P4 bids; G AND H get score → bid fails
    private Guid _p2CounterPickTargetMasterGameID;     // P2 CP bids; gets score → CP bid fails

    // ── P3 drop targets ───────────────────────────────────────────────────────
    private Guid _p3StandaloneDropPublisherGameID;
    private Guid _p3StandaloneDropMasterGameID;        // stays clean → drop succeeds
    private Guid _p3ConditionalDropPublisherGameID;
    private Guid _p3ConditionalDropMasterGameID;       // stays clean → condrop succeeds

    // ── P4 drop targets ───────────────────────────────────────────────────────
    private Guid _p4StandaloneDropPublisherGameID;
    private Guid _p4StandaloneDropMasterGameID;        // gets score → drop fails
    private Guid _p4ConditionalDropPublisherGameID;
    private Guid _p4ConditionalDropMasterGameID;       // gets score → condrop fails; bid also fails because G has score

    // ── Post-processing state ─────────────────────────────────────────────────
    private LeagueYearViewModel _postProcessingSnapshot = null!;
    private LeagueActionProcessingSetViewModel _actionSet = null!;

    [OneTimeSetUp]
    public async Task SetUpEligibilityChangeLeague()
    {
        // ── Phase 1: Clock and draft ──────────────────────────────────────────

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
        _leagueID = await LeagueTestHelpers.CreateLeagueAsync(
            _managerSession, LeagueScenarios.FourPlayerEligibilityChange, _year);

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

        // ── Phase 2: Capture post-draft state ─────────────────────────────────

        var postDraftSnapshot = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);
        _p1StartBudget = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID).Budget;
        _p2StartBudget = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _p2PublisherID).Budget;

        var p3Publisher = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID);
        var p4Publisher = postDraftSnapshot.Publishers.Single(p => p.PublisherID == _p4PublisherID);

        // ── Phase 3: Identify bid targets ─────────────────────────────────────

        var usedMasterGameIDs = new HashSet<Guid>();

        var p1Available = await _managerSession.League.TopAvailableGamesAsync(_year, _leagueID, _p1PublisherID, null);
        _gameAMasterGameID = await SelectBidTargetAsync(p1Available, usedMasterGameIDs);
        usedMasterGameIDs.Add(_gameAMasterGameID);

        var p2Available = await _p2Session.League.TopAvailableGamesAsync(_year, _leagueID, _p2PublisherID, null);
        _gameBMasterGameID = await SelectBidTargetAsync(p2Available, usedMasterGameIDs);
        usedMasterGameIDs.Add(_gameBMasterGameID);

        var p3Available = await _p3Session.League.TopAvailableGamesAsync(_year, _leagueID, _p3PublisherID, null);
        _gameEMasterGameID = await SelectBidTargetAsync(p3Available, usedMasterGameIDs);
        usedMasterGameIDs.Add(_gameEMasterGameID);

        var p4Available = await _p4Session.League.TopAvailableGamesAsync(_year, _leagueID, _p4PublisherID, null);
        _gameGMasterGameID = await SelectBidTargetAsync(p4Available, usedMasterGameIDs);

        // Counter-pick target: any game another publisher has that P2 can CP bid on
        var possibleCPs = await _p2Session.League.PossibleCounterPicksAsync(_p2PublisherID);
        _p2CounterPickTargetMasterGameID = await SelectCounterPickTargetAsync(possibleCPs);

        // ── Phase 4: Identify drop targets ────────────────────────────────────

        // P3 needs two distinct droppable drafted games (one standalone, one conditional).
        var p3DroppableGames = p3Publisher.Games
            .Where(g => !g.CounterPick && !g.DropBlocked
                && g.MasterGame != null && g.OverallDraftPosition.HasValue)
            .ToList();
        if (p3DroppableGames.Count < 2)
            throw new InvalidOperationException(
                "P3 needs at least 2 droppable drafted games for the eligibility-change test setup.");
        _p3StandaloneDropPublisherGameID = p3DroppableGames[0].PublisherGameID;
        _p3StandaloneDropMasterGameID    = p3DroppableGames[0].MasterGame!.MasterGameID;
        _p3ConditionalDropPublisherGameID = p3DroppableGames[1].PublisherGameID;
        _p3ConditionalDropMasterGameID    = p3DroppableGames[1].MasterGame!.MasterGameID;

        // P4 needs two distinct droppable drafted games.
        var p4DroppableGames = p4Publisher.Games
            .Where(g => !g.CounterPick && !g.DropBlocked
                && g.MasterGame != null && g.OverallDraftPosition.HasValue)
            .ToList();
        if (p4DroppableGames.Count < 2)
            throw new InvalidOperationException(
                "P4 needs at least 2 droppable drafted games for the eligibility-change test setup.");
        _p4StandaloneDropPublisherGameID = p4DroppableGames[0].PublisherGameID;
        _p4StandaloneDropMasterGameID    = p4DroppableGames[0].MasterGame!.MasterGameID;
        _p4ConditionalDropPublisherGameID = p4DroppableGames[1].PublisherGameID;
        _p4ConditionalDropMasterGameID    = p4DroppableGames[1].MasterGame!.MasterGameID;

        // ── Phase 5: Place bids and drops ─────────────────────────────────────

        await PlaceBidAsync(_managerSession, _p1PublisherID, _gameAMasterGameID, 10, false, null);
        await PlaceBidAsync(_p2Session, _p2PublisherID, _gameBMasterGameID, 10, false, null);
        await PlaceDropAsync(_p3Session, _p3PublisherID, _p3StandaloneDropPublisherGameID);
        await PlaceBidAsync(_p3Session, _p3PublisherID, _gameEMasterGameID, 10, false, _p3ConditionalDropPublisherGameID);
        await PlaceDropAsync(_p4Session, _p4PublisherID, _p4StandaloneDropPublisherGameID);
        await PlaceBidAsync(_p4Session, _p4PublisherID, _gameGMasterGameID, 10, false, _p4ConditionalDropPublisherGameID);
        await PlaceBidAsync(_p2Session, _p2PublisherID, _p2CounterPickTargetMasterGameID, 5, true, null);

        // ── Phase 6: Advance to T-28h; inject scores into targeted games ──────
        // Saturday 20:00 ET = Sunday 01:00 UTC (Jan 12).
        // We set Friday Jan 10 20:00 UTC — well before the window — to inject scores.

        await _adminSession.Admin.SetTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 10, 20, 0, 0, TimeSpan.Zero)
        });

        await EditMasterGameToAddScoreAsync(_gameBMasterGameID);
        await EditMasterGameToAddScoreAsync(_gameGMasterGameID);
        await EditMasterGameToAddScoreAsync(_p4StandaloneDropMasterGameID);
        await EditMasterGameToAddScoreAsync(_p4ConditionalDropMasterGameID);
        await EditMasterGameToAddScoreAsync(_p2CounterPickTargetMasterGameID);

        // ── Phase 7: Advance to processing time, run actions ──────────────────
        // Saturday Jan 11, 2025 20:01 ET = Jan 12, 2025 01:01 UTC (ET is UTC-5 in January)

        await _adminSession.Admin.SetTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 12, 1, 1, 0, TimeSpan.Zero)
        });

        await _adminSession.ActionRunner.TurnOnActionProcessingModeAsync();
        await _adminSession.ActionRunner.ProcessActionsAsync();
        await _adminSession.ActionRunner.TurnOffActionProcessingModeAsync();

        // ── Phase 8: Capture post-processing state ────────────────────────────

        _postProcessingSnapshot = await _adminSession.League.GetLeagueYearAsync(_leagueID, _year, null);
        var actionSets = await _adminSession.League.GetLeagueActionSetsAsync(_leagueID, _year);
        _actionSet = actionSets.Single();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        // Reset any injected scores so they don't bleed into future test runs.
        // EditMasterGame updates MasterGame but NOT MasterGameYear, so the score persists
        // in the live MasterGame check used by PlaceBidAsync even though TopAvailableGamesAsync
        // may show HasScore = false (stale MasterGameYear). Cleaning here keeps the DB fresh.
        if (_adminSession != null)
        {
            await SafeResetMasterGameScoreAsync(_gameBMasterGameID);
            await SafeResetMasterGameScoreAsync(_gameGMasterGameID);
            await SafeResetMasterGameScoreAsync(_p4StandaloneDropMasterGameID);
            await SafeResetMasterGameScoreAsync(_p4ConditionalDropMasterGameID);
            await SafeResetMasterGameScoreAsync(_p2CounterPickTargetMasterGameID);

            await _adminSession.ActionRunner.TurnOffActionProcessingModeAsync();
            await _adminSession.Admin.ResetTimeAsync();
        }
        _adminSession?.Dispose();
        _managerSession?.Dispose();
        _p2Session?.Dispose();
        _p3Session?.Dispose();
        _p4Session?.Dispose();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Fetches the current master game state and re-saves it with CriticScore = 85.0
    /// injected, leaving all other fields unchanged.
    /// </summary>
    private async Task EditMasterGameToAddScoreAsync(Guid masterGameID)
    {
        var game = await _adminSession.Game.MasterGameAsync(masterGameID);
        await _adminSession.FactChecker.EditMasterGameAsync(new EditMasterGameRequest
        {
            MasterGameID               = masterGameID,
            GameName                   = game.GameName,
            EstimatedReleaseDate       = game.EstimatedReleaseDate,
            MinimumReleaseDate         = game.MinimumReleaseDate,
            MaximumReleaseDate         = game.MaximumReleaseDate,
            EarlyAccessReleaseDate     = game.EarlyAccessReleaseDate,
            InternationalReleaseDate   = game.InternationalReleaseDate,
            AnnouncementDate           = game.AnnouncementDate,
            ReleaseDate                = game.ReleaseDate,
            Tags                       = game.Tags.ToList(),
            SyncWithExternalAPIs       = game.SyncWithExternalAPIs,
            UseSimpleEligibility       = game.UseSimpleEligibility,
            DelayContention            = game.DelayContention,
            ShowNote                   = game.ShowNote,
            OpenCriticID               = game.OpenCriticID,
            GgToken                    = game.GgToken,
            Notes                      = game.Notes,
            CriticScore                = 85.0m,
        });
    }

    /// <summary>
    /// Resets a master game's CriticScore to null if the game ID is non-empty.
    /// Called from TearDown to prevent scores injected in this run from persisting
    /// in the live MasterGame table and blocking future test runs.
    /// </summary>
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
                MasterGameID               = masterGameID,
                GameName                   = game.GameName,
                EstimatedReleaseDate       = game.EstimatedReleaseDate,
                MinimumReleaseDate         = game.MinimumReleaseDate,
                MaximumReleaseDate         = game.MaximumReleaseDate,
                EarlyAccessReleaseDate     = game.EarlyAccessReleaseDate,
                InternationalReleaseDate   = game.InternationalReleaseDate,
                AnnouncementDate           = game.AnnouncementDate,
                ReleaseDate                = game.ReleaseDate,
                Tags                       = game.Tags.ToList(),
                SyncWithExternalAPIs       = game.SyncWithExternalAPIs,
                UseSimpleEligibility       = game.UseSimpleEligibility,
                DelayContention            = game.DelayContention,
                ShowNote                   = game.ShowNote,
                OpenCriticID               = game.OpenCriticID,
                GgToken                    = game.GgToken,
                Notes                      = game.Notes,
                CriticScore                = null,
                ClearCriticScore           = true,
            });
        }
        catch
        {
            // Best-effort: do not let TearDown failures mask test failures.
        }
    }

    /// <summary>
    /// Selects the first available, untaken, unreleased game that has no live critic score.
    /// Queries MasterGameAsync directly to check the live MasterGame score, since
    /// TopAvailableGamesAsync uses the potentially-stale MasterGameYear table.
    /// </summary>
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

    /// <summary>
    /// Selects the first counter-pick candidate whose game has no live critic score.
    /// </summary>
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
        ApiSession session,
        Guid publisherID,
        Guid masterGameID,
        int bidAmount,
        bool counterPick,
        Guid? conditionalDropPublisherGameID)
    {
        var result = await session.League.MakePickupBidAsync(new PickupBidRequest
        {
            PublisherID                    = publisherID,
            MasterGameID                   = masterGameID,
            CounterPick                    = counterPick,
            BidAmount                      = bidAmount,
            AllowIneligibleSlot            = false,
            ConditionalDropPublisherGameID = conditionalDropPublisherGameID,
        });
        if (!result.Success)
        {
            var errors = string.Join("; ", result.Errors ?? []);
            throw new InvalidOperationException(
                $"MakePickupBid failed for publisher {publisherID}, game {masterGameID}, " +
                $"amount {bidAmount}, counterPick {counterPick}. Errors: {errors}");
        }
    }

    private static async Task PlaceDropAsync(
        ApiSession session,
        Guid publisherID,
        Guid publisherGameID)
    {
        var result = await session.League.MakeDropRequestAsync(new DropGameRequestRequest
        {
            PublisherID     = publisherID,
            PublisherGameID = publisherGameID,
        });
        if (!result.Success)
        {
            var errors = string.Join("; ", result.Errors ?? []);
            throw new InvalidOperationException(
                $"MakeDropRequest failed for publisher {publisherID}, " +
                $"publisherGame {publisherGameID}. Errors: {errors}");
        }
    }

    // ── Tests — Roster state ──────────────────────────────────────────────────

    [Test]
    public void Bid_EligibleGame_Succeeds()
    {
        var p1 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID);
        Assert.That(
            p1.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _gameAMasterGameID),
            Is.True,
            "P1 should have Game A after the uncontested bid succeeds.");
    }

    [Test]
    public void Bid_EligibleGame_BudgetDeducted()
    {
        var p1 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID);
        Assert.That(p1.Budget, Is.EqualTo(_p1StartBudget - 10),
            "P1's budget should be reduced by $10.");
    }

    [Test]
    public void Bid_ScoredGame_Fails_NotOnRoster()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p2PublisherID);
        Assert.That(
            p2.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _gameBMasterGameID),
            Is.False,
            "P2 should NOT have Game B — the game gained a score before processing.");
    }

    [Test]
    public void Bid_ScoredGame_Fails_BudgetUnchanged()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p2PublisherID);
        Assert.That(p2.Budget, Is.EqualTo(_p2StartBudget),
            "P2's budget should be unchanged — all P2 bids failed.");
    }

    [Test]
    public void Drop_EligibleGame_Succeeds_NotOnRoster()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID);
        Assert.That(
            p3.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _p3StandaloneDropMasterGameID),
            Is.False,
            "P3's standalone-dropped game should no longer be on the roster.");
    }

    [Test]
    public void Drop_EligibleGame_Succeeds_InFormerGames()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID);
        Assert.That(
            p3.FormerGames.Any(g => g.MasterGame?.MasterGameID == _p3StandaloneDropMasterGameID),
            Is.True,
            "P3's standalone-dropped game should appear in FormerGames.");
    }

    [Test]
    public void Drop_ScoredGame_Fails_StaysOnRoster()
    {
        var p4 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p4PublisherID);
        Assert.That(
            p4.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _p4StandaloneDropMasterGameID),
            Is.True,
            "P4's drop target should still be on the roster — it gained a score before processing.");
    }

    [Test]
    public void ConditionalDrop_EligibleGame_Succeeds_BidGameOnRoster()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID);
        Assert.That(
            p3.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _gameEMasterGameID),
            Is.True,
            "P3 should have Game E after the conditional-drop bid succeeds.");
    }

    [Test]
    public void ConditionalDrop_EligibleGame_Succeeds_DroppedGameGone()
    {
        var p3 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p3PublisherID);
        Assert.That(
            p3.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _p3ConditionalDropMasterGameID),
            Is.False,
            "P3's conditionally-dropped game should no longer be on the roster.");
    }

    [Test]
    public void ConditionalDrop_ScoredGame_Fails_BidGameNotOnRoster()
    {
        var p4 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p4PublisherID);
        Assert.That(
            p4.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _gameGMasterGameID),
            Is.False,
            "P4 should NOT have Game G — both G and H (conditional drop) gained a score, causing the bid to fail.");
    }

    [Test]
    public void ConditionalDrop_ScoredGame_Fails_GameStaysOnRoster()
    {
        var p4 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p4PublisherID);
        Assert.That(
            p4.Games.Any(g => !g.CounterPick && g.MasterGame?.MasterGameID == _p4ConditionalDropMasterGameID),
            Is.True,
            "P4's conditional-drop game (H) should still be on the roster — it gained a score so the drop was rejected.");
    }

    [Test]
    public void CounterPickBid_ScoredGame_Fails_NotOnRoster()
    {
        var p2 = _postProcessingSnapshot.Publishers.Single(p => p.PublisherID == _p2PublisherID);
        Assert.That(
            p2.Games.Any(g => g.CounterPick && g.MasterGame?.MasterGameID == _p2CounterPickTargetMasterGameID),
            Is.False,
            "P2 should NOT have the counter-pick — the target game gained a score before processing.");
    }

    // ── Tests — History ───────────────────────────────────────────────────────

    [Test]
    public void History_Bid_EligibleGame_Successful()
    {
        var bid = _actionSet.Bids.Single(b =>
            !b.CounterPick && b.MasterGame.MasterGameID == _gameAMasterGameID);
        Assert.That(bid.Successful, Is.True,
            "The action history should record Game A's bid as successful.");
    }

    [Test]
    public void History_Bid_ScoredGame_Unsuccessful_WithScoreOutcome()
    {
        var bid = _actionSet.Bids.Single(b =>
            !b.CounterPick && b.MasterGame.MasterGameID == _gameBMasterGameID);
        Assert.That(bid.Successful, Is.False,
            "The action history should record Game B's bid as unsuccessful.");
        Assert.That(bid.Outcome, Does.Contain("score").IgnoreCase,
            "The failure outcome should mention 'score'.");
    }

    [Test]
    public void History_Drop_EligibleGame_Successful()
    {
        var drop = _actionSet.Drops.Single(d =>
            d.PublisherID == _p3PublisherID
            && d.MasterGame.MasterGameID == _p3StandaloneDropMasterGameID);
        Assert.That(drop.Successful, Is.True,
            "The action history should record P3's standalone drop as successful.");
    }

    [Test]
    public void History_Drop_ScoredGame_Unsuccessful()
    {
        var drop = _actionSet.Drops.Single(d =>
            d.PublisherID == _p4PublisherID
            && d.MasterGame.MasterGameID == _p4StandaloneDropMasterGameID);
        Assert.That(drop.Successful, Is.False,
            "The action history should record P4's drop as unsuccessful — the game gained a score.");
    }

    [Test]
    public void History_ConditionalDropBid_ScoredGame_Unsuccessful_WithScoreOutcome()
    {
        var bid = _actionSet.Bids.Single(b =>
            !b.CounterPick && b.MasterGame.MasterGameID == _gameGMasterGameID);
        Assert.That(bid.Successful, Is.False,
            "The action history should record Game G's bid as unsuccessful — G gained a score before processing.");
        Assert.That(bid.Outcome, Does.Contain("score").IgnoreCase,
            "The failure outcome should mention 'score'.");
    }

    [Test]
    public void History_CounterPickBid_ScoredGame_Unsuccessful_WithScoreOutcome()
    {
        var bid = _actionSet.Bids.Single(b =>
            b.CounterPick && b.MasterGame.MasterGameID == _p2CounterPickTargetMasterGameID);
        Assert.That(bid.Successful, Is.False,
            "The action history should record the counter-pick bid as unsuccessful.");
        Assert.That(bid.Outcome, Does.Contain("score").IgnoreCase,
            "The failure outcome should mention 'score'.");
    }
}
