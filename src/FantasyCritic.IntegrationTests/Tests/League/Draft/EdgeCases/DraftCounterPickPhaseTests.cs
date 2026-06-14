using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Draft.EdgeCases;

/// <summary>
/// Tests error conditions during the counter-pick phase of a draft.
///
/// OneTimeSetUp creates a <see cref="LeagueScenarios.TwoPlayerSmall"/> league, drives all
/// four standard game picks using <see cref="LeagueFixture.DraftUntilCounterPickPhaseAsync"/>,
/// then records which publisher is first to counter-pick. Tests make only rejected attempts
/// and do not advance the shared draft state.
/// </summary>
[TestFixture]
public class DraftCounterPickPhaseTests : IntegrationTestBase
{
    private LeagueFixture _league = null!;
    private TestPublisher _firstCpPublisher = null!;
    private PossibleMasterGameYearViewModel _availableUndraftedGame = null!;

    [OneTimeSetUp]
    public async Task SetUpLeagueAtCounterPickPhase()
    {
        _league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, LeagueScenarios.TwoPlayerSmall, NewUser);

        await _league.DraftUntilCounterPickPhaseAsync();

        var cpPhaseSnapshot = await _league.GetLeagueYearAsync();
        var firstCpPublisher = cpPhaseSnapshot.Publishers.Single(p => p.NextToDraft);
        _firstCpPublisher = _league.Publishers.Single(p => p.PublisherID == firstCpPublisher.PublisherID);

        var available = await _firstCpPublisher.Session.League.TopAvailableGamesAsync(
            _league.Year, _league.LeagueID, _firstCpPublisher.PublisherID, slotInfo: null);
        _availableUndraftedGame = available.First(g => !g.Taken);
    }

    [OneTimeTearDown]
    public async Task TearDown() => await _league.DisposeAsync();

    [Test]
    public async Task DraftGame_StandardGameDuringCounterPickPhase_Throws()
    {
        ApiException? ex = null;
        try
        {
            await _firstCpPublisher.Session.League.DraftGameAsync(new DraftGameRequest
            {
                PublisherID = _firstCpPublisher.PublisherID,
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

    [Test]
    public async Task DraftGame_CounterPickOfUnownedGame_ReturnsFailureResult()
    {
        var result = await _firstCpPublisher.Session.League.DraftGameAsync(new DraftGameRequest
        {
            PublisherID = _firstCpPublisher.PublisherID,
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
