using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Actions;

/// <summary>
/// Integration tests for counter picking a game no publisher in the league owns: each acquisition path
/// returns the eligibility error instead of failing the request.
/// </summary>
[TestFixture]
public class CounterPickUnownedGameTests : IntegrationTestBase
{
    private const string NoOtherPlayerPublishingError = "Cannot counter pick a game that no other player is publishing.";

    private ApiSession _adminSession = null!;
    private LeagueFixture _league = null!;
    private Guid _unownedMasterGameID;

    [OneTimeSetUp]
    public async Task SetUpLeagueWithOpenCounterPickSlots()
    {
        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);

        await _adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 6, 12, 0, 0, TimeSpan.Zero)
        });

        var scenario = new LeagueScenario
        {
            Name = "CounterPickUnownedGame",
            PlayerCount = LeagueScenarios.FourPlayerDrops.PlayerCount,
            StandardGames = LeagueScenarios.FourPlayerDrops.StandardGames,
            GamesToDraft = LeagueScenarios.FourPlayerDrops.GamesToDraft,
            CounterPicks = LeagueScenarios.FourPlayerDrops.CounterPicks,
            CounterPicksToDraft = 0,
            PickupSystem = LeagueScenarios.FourPlayerDrops.PickupSystem,
            ScoringSystem = LeagueScenarios.FourPlayerDrops.ScoringSystem,
            TradingSystem = LeagueScenarios.FourPlayerDrops.TradingSystem,
            TiebreakSystem = LeagueScenarios.FourPlayerDrops.TiebreakSystem,
            ReleaseSystem = LeagueScenarios.FourPlayerDrops.ReleaseSystem,
            IneligibleGameSystem = LeagueScenarios.FourPlayerDrops.IneligibleGameSystem,
            UnrestrictedReleaseStatusDroppableGames = LeagueScenarios.FourPlayerDrops.UnrestrictedReleaseStatusDroppableGames,
            WillNotReleaseDroppableGames = LeagueScenarios.FourPlayerDrops.WillNotReleaseDroppableGames,
            WillReleaseDroppableGames = LeagueScenarios.FourPlayerDrops.WillReleaseDroppableGames,
            DropOnlyDraftGames = LeagueScenarios.FourPlayerDrops.DropOnlyDraftGames,
            GrantSuperDrops = LeagueScenarios.FourPlayerDrops.GrantSuperDrops,
            CounterPicksBlockDrops = LeagueScenarios.FourPlayerDrops.CounterPicksBlockDrops,
            AllowMoveIntoIneligible = LeagueScenarios.FourPlayerDrops.AllowMoveIntoIneligible,
            MinimumBidAmount = LeagueScenarios.FourPlayerDrops.MinimumBidAmount,
            EnableBids = LeagueScenarios.FourPlayerDrops.EnableBids,
        };

        _league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, scenario, NewUser);
        await _league.DraftToCompletionAsync();

        var publisher = _league.Publishers[0];
        var available = await publisher.Session.League.TopAvailableGamesAsync(
            _league.Year, _league.LeagueID, publisher.PublisherID, null);
        var unownedGame = available.First(g => g.IsAvailable && !g.Taken && !g.IsReleased && g.MasterGame != null);
        _unownedMasterGameID = unownedGame.MasterGame!.MasterGameID;
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await _adminSession.Admin.ResetTimeAsync();
        _adminSession.Dispose();
        await _league.DisposeAsync();
    }

    [Test]
    public async Task ManagerClaim_CounterPickOnUnownedGame_ReturnsClaimError()
    {
        var publisher = _league.Publishers[0];

        var result = await _league.Manager.LeagueManager.ManagerClaimGameAsync(new ClaimGameRequest
        {
            PublisherID = publisher.PublisherID,
            GameName = "Unowned Game",
            MasterGameID = _unownedMasterGameID,
            CounterPick = true,
            ManagerOverride = false,
            AllowIneligibleSlot = false,
        });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Errors, Does.Contain(NoOtherPlayerPublishingError));
        }
    }

    [Test]
    public async Task PickupBid_CounterPickOnUnownedGame_ReturnsClaimError()
    {
        var publisher = _league.Publishers[0];

        var result = await LeaguePickupActions.TryPlaceBidAsync(publisher, _unownedMasterGameID, 10, counterPick: true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Errors, Does.Contain(NoOtherPlayerPublishingError));
        }
    }
}
