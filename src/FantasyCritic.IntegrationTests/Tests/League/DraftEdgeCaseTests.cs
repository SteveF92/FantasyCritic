using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League;

/// <summary>
/// Tests error conditions during the standard-game phase of a draft.
///
/// OneTimeSetUp creates a <see cref="LeagueScenarios.TwoPlayerSmall"/> league, starts the draft,
/// and has P1 (draft position 1) make exactly one standard pick. All tests share that
/// mid-draft state (P2 is next, P1 has 1 game) and make only failed / rejected attempts
/// so they do not advance the shared draft state.
/// </summary>
[TestFixture]
public class DraftEdgeCaseTests : IntegrationTestBase
{
    private ApiSession _managerSession = null!;
    private ApiSession _playerSession = null!;
    private Guid _leagueID;
    private int _year;
    private Guid _p1PublisherID;
    private Guid _p2PublisherID;
    private Guid _p1DraftedGameID;
    private PossibleMasterGameYearViewModel? _ineligibleGame;

    [OneTimeSetUp]
    public async Task SetUpLeagueMidDraft()
    {
        var (mgrEmail, mgrPassword, mgrDisplayName) = NewUser();
        _managerSession = new ApiSession(Factory);
        await _managerSession.RegisterAsync(mgrEmail, mgrPassword, mgrDisplayName);

        var (plrEmail, plrPassword, plrDisplayName) = NewUser();
        _playerSession = new ApiSession(Factory);
        await _playerSession.RegisterAsync(plrEmail, plrPassword, plrDisplayName);

        IReadOnlyList<Guid> publisherIDs;
        IReadOnlyDictionary<Guid, ApiSession> publisherSessionMap;
        (_leagueID, _year, publisherIDs, publisherSessionMap) =
            await LeagueTestHelpers.SetUpLeagueAndStartDraftAsync(
                _managerSession, [_playerSession], LeagueScenarios.TwoPlayerSmall);

        // [0] = manager (draft position 1), [1] = player (draft position 2)
        _p1PublisherID = publisherIDs[0];
        _p2PublisherID = publisherIDs[1];

        // P1 makes exactly one standard pick (MockedLivePlayer picks the first available game).
        var p1Player = new MockedLivePlayer(_managerSession, _p1PublisherID, _leagueID);
        await p1Player.DraftStandardGameAsync(_year);

        // Read back which game P1 picked so duplicate-game tests can target it.
        var snapshot = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);
        var p1Publisher = snapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID);
        _p1DraftedGameID = p1Publisher.Games
            .Single(g => !g.CounterPick).MasterGame!.MasterGameID;

        // Record the first ineligible-but-available game for P2 (null if none found).
        // Games with IsEligible == false are ineligible under the league's ReleaseSystem =
        // "MustBeReleased" setting — typically unreleased titles not expected this year.
        var available = await _playerSession.League.TopAvailableGamesAsync(
            _year, _leagueID, _p2PublisherID, slotInfo: null);
        _ineligibleGame = available.FirstOrDefault(g => !g.IsEligible && !g.Taken);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _managerSession?.Dispose();
        _playerSession?.Dispose();
    }

    // ── Tests ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// It is P2's turn. P1 calls DraftGame using their own publisher ID.
    /// The server should reject with 400 "It is not your turn to draft."
    /// </summary>
    [Test]
    public async Task DraftGame_OutOfTurn_Throws()
    {
        ApiException? ex = null;
        try
        {
            await _managerSession.League.DraftGameAsync(new DraftGameRequest
            {
                PublisherID = _p1PublisherID,
                MasterGameID = null,
                GameName = "out-of-turn-attempt",
                CounterPick = false,
                AllowIneligibleSlot = false,
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null, "Expected ApiException when drafting out of turn.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }

    /// <summary>
    /// P2 calls DraftGame with CounterPick = true while still in the standard-game phase.
    /// The server should reject with 400 "Not drafting counterPicks now."
    /// </summary>
    [Test]
    public async Task DraftGame_CounterPickFlagDuringStandardPhase_Throws()
    {
        ApiException? ex = null;
        try
        {
            await _playerSession.League.DraftGameAsync(new DraftGameRequest
            {
                PublisherID = _p2PublisherID,
                MasterGameID = null,
                GameName = "wrong-phase-attempt",
                CounterPick = true,
                AllowIneligibleSlot = false,
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null, "Expected ApiException when using CounterPick flag in standard phase.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }

    /// <summary>
    /// P2 tries to pick the same game P1 already has on their roster.
    /// The service returns Success = false (HTTP 200 with an error body).
    /// </summary>
    [Test]
    public async Task DraftGame_DuplicateGame_ReturnsFailureResult()
    {
        var result = await _playerSession.League.DraftGameAsync(new DraftGameRequest
        {
            PublisherID = _p2PublisherID,
            MasterGameID = _p1DraftedGameID,
            GameName = "duplicate-game-attempt",
            CounterPick = false,
            AllowIneligibleSlot = false,
        });

        Assert.That(result.Success, Is.False,
            "Picking a game already on another publisher's roster should return Success = false.");
        Assert.That(result.Errors, Is.Not.Empty,
            "Errors should be non-empty when a duplicate game pick fails.");
    }

    /// <summary>
    /// P2 tries to draft a game that is ineligible in this league's slot (IsEligible = false).
    /// The service returns Success = false (HTTP 200 with an error body).
    /// Skipped if no ineligible games appear in TopAvailableGames for P2's publisher.
    /// </summary>
    [Test]
    public async Task DraftGame_IneligibleGame_ReturnsFailureResult()
    {
        Assume.That(_ineligibleGame, Is.Not.Null,
            "No ineligible game found in TopAvailableGames for P2; test skipped.");

        var result = await _playerSession.League.DraftGameAsync(new DraftGameRequest
        {
            PublisherID = _p2PublisherID,
            MasterGameID = _ineligibleGame!.MasterGame.MasterGameID,
            GameName = _ineligibleGame.MasterGame.GameName,
            CounterPick = false,
            AllowIneligibleSlot = false,
        });

        Assert.That(result.Success, Is.False,
            "Drafting an ineligible game with AllowIneligibleSlot = false should return Success = false.");
        Assert.That(result.Errors, Is.Not.Empty,
            "Errors should be non-empty when an ineligible game pick fails.");
    }
}
