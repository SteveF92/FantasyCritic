using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Draft.EdgeCases;

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
    private LeagueFixture _league = null!;
    private Guid _p1DraftedGameID;
    private PossibleMasterGameYearViewModel? _ineligibleGame;

    [OneTimeSetUp]
    public async Task SetUpLeagueMidDraft()
    {
        _league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, LeagueScenarios.TwoPlayerSmall, NewUser);

        await _league.DraftStandardPicksAsync(1);

        var snapshot = await _league.GetLeagueYearAsync();
        var p1Publisher = snapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[0].PublisherID);
        _p1DraftedGameID = p1Publisher.Games
            .Single(g => !g.CounterPick).MasterGame!.MasterGameID;

        var searchResults = await _league.Publishers[1].Session.League.PossibleMasterGamesAsync(
            "a", _league.Year, _league.LeagueID);
        _ineligibleGame = searchResults.FirstOrDefault(g => !g.IsEligible && !g.Taken && !g.AlreadyOwned);
    }

    [OneTimeTearDown]
    public async Task TearDown() => await _league.DisposeAsync();

    [Test]
    public async Task DraftGame_OutOfTurn_Throws()
    {
        ApiException? ex = null;
        try
        {
            await _league.Publishers[0].Session.League.DraftGameAsync(new DraftGameRequest
            {
                PublisherID = _league.Publishers[0].PublisherID,
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

    [Test]
    public async Task DraftGame_CounterPickFlagDuringStandardPhase_Throws()
    {
        ApiException? ex = null;
        try
        {
            await _league.Publishers[1].Session.League.DraftGameAsync(new DraftGameRequest
            {
                PublisherID = _league.Publishers[1].PublisherID,
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

    [Test]
    public async Task DraftGame_DuplicateGame_ReturnsFailureResult()
    {
        var result = await _league.Publishers[1].Session.League.DraftGameAsync(new DraftGameRequest
        {
            PublisherID = _league.Publishers[1].PublisherID,
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

    [Test]
    public async Task DraftGame_IneligibleGame_ReturnsFailureResult()
    {
        Assume.That(_ineligibleGame, Is.Not.Null,
            "No ineligible game found via PossibleMasterGames search; test skipped.");

        var result = await _league.Publishers[1].Session.League.DraftGameAsync(new DraftGameRequest
        {
            PublisherID = _league.Publishers[1].PublisherID,
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
