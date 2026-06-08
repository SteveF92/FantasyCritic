using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League;

/// <summary>
/// Tests error conditions during the counter-pick phase of a draft.
///
/// OneTimeSetUp creates a <see cref="LeagueScenarios.TwoPlayerSmall"/> league, drives all
/// four standard game picks using <see cref="DraftSimulator.RunUntilCounterPickPhaseAsync"/>,
/// then records which publisher is first to counter-pick. Tests make only rejected attempts
/// and do not advance the shared draft state.
/// </summary>
[TestFixture]
public class DraftCounterPickPhaseTests : IntegrationTestBase
{
    private ApiSession _managerSession = null!;
    private ApiSession _playerSession = null!;
    private Guid _leagueID;
    private int _year;
    private Guid _firstCpPublisherID;
    private ApiSession _firstCpSession = null!;
    private PossibleMasterGameYearViewModel _availableUndraftedGame = null!;

    [OneTimeSetUp]
    public async Task SetUpLeagueAtCounterPickPhase()
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

        // Drive all 4 standard picks (2 players × 2 games each); stop when counter-pick phase starts.
        var players = publisherSessionMap.Select(kvp =>
            new MockedLivePlayer(kvp.Value, kvp.Key, _leagueID));
        var simulator = new DraftSimulator(_managerSession, players);
        await simulator.RunUntilCounterPickPhaseAsync(_leagueID, _year);

        // Determine who is first to pick a counter-pick (don't assume draft position 1 always goes first).
        var cpPhaseSnapshot = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);
        var firstCpPublisher = cpPhaseSnapshot.Publishers.Single(p => p.NextToDraft);
        _firstCpPublisherID = firstCpPublisher.PublisherID;
        _firstCpSession = publisherSessionMap[_firstCpPublisherID];

        // Find a game not on any publisher's roster — an invalid counter-pick target.
        // Counter-picks must be from another publisher's standard roster; a still-available
        // (Taken == false) game was never drafted and is therefore invalid.
        var available = await _firstCpSession.League.TopAvailableGamesAsync(
            _year, _leagueID, _firstCpPublisherID, slotInfo: null);
        _availableUndraftedGame = available.First(g => !g.Taken);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _managerSession?.Dispose();
        _playerSession?.Dispose();
    }

    // ── Tests ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// The first counter-pick publisher attempts to draft a standard game (CounterPick = false)
    /// during the counter-pick phase. The server returns 400 "Not drafting standard games now."
    /// </summary>
    [Test]
    public async Task DraftGame_StandardGameDuringCounterPickPhase_Throws()
    {
        ApiException? ex = null;
        try
        {
            await _firstCpSession.League.DraftGameAsync(new DraftGameRequest
            {
                PublisherID = _firstCpPublisherID,
                MasterGameID = null,
                GameName = "wrong-phase-standard-attempt",
                CounterPick = false,
                AllowIneligibleSlot = false,
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null, "Expected ApiException when using standard-game flag in counter-pick phase.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }

    /// <summary>
    /// The first counter-pick publisher attempts to counter-pick a game that is not on any
    /// publisher's standard roster. Counter-picks must target another publisher's drafted game;
    /// a game that was never drafted is an invalid target.
    /// The service returns Success = false (HTTP 200 with an error body).
    /// </summary>
    [Test]
    public async Task DraftGame_CounterPickOfUnownedGame_ReturnsFailureResult()
    {
        var result = await _firstCpSession.League.DraftGameAsync(new DraftGameRequest
        {
            PublisherID = _firstCpPublisherID,
            MasterGameID = _availableUndraftedGame.MasterGame.MasterGameID,
            GameName = _availableUndraftedGame.MasterGame.GameName,
            CounterPick = true,
            AllowIneligibleSlot = false,
        });

        Assert.That(result.Success, Is.False,
            "Counter-picking a game not on any publisher's roster should return Success = false.");
        Assert.That(result.Errors, Is.Not.Empty,
            "Errors should be non-empty when an invalid counter-pick target is used.");
    }
}
