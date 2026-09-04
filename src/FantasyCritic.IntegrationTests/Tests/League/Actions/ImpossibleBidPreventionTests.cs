using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using FantasyCritic.Lib.BusinessLogicFunctions;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Actions;

/// <summary>
/// Integration tests for impossible bid prevention: full-roster bids without a slot
/// acquisition path are rejected at placement, edit, and drop-cancellation time.
/// </summary>
[TestFixture]
public class ImpossibleBidPreventionTests : IntegrationTestBase
{
    private ApiSession _adminSession = null!;
    private LeagueFixture? _league;

    [SetUp]
    public async Task SetUp()
    {
        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);

        await _adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 6, 12, 0, 0, TimeSpan.Zero)
        });
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_adminSession != null)
        {
            await _adminSession.ActionRunner.TurnOffActionProcessingModeAsync();
            await _adminSession.Admin.ResetTimeAsync();
        }

        _adminSession?.Dispose();

        if (_league != null)
        {
            await _league.DisposeAsync();
            _league = null;
        }
    }

    [Test]
    public async Task FullRoster_BidWithoutDropOrConditional_Rejected()
    {
        var league = await CreateFullRosterLeagueAsync();
        var publisher = league.Publishers[0];
        var targetMasterGameID = await PickAvailableBidTargetAsync(league, publisher, []);

        var result = await LeaguePickupActions.TryPlaceBidAsync(publisher, targetMasterGameID, 10, false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Errors, Does.Contain(BidSlotPathFunctions.NoSlotPathError));
        }
    }

    [Test]
    public async Task FullRoster_PendingDrop_AllowsBid()
    {
        var league = await CreateFullRosterLeagueAsync();
        var publisher = league.Publishers[0];
        var snapshot = await publisher.Session.League.GetLeagueYearAsync(league.LeagueID, league.Year, null);
        var publisherViewModel = snapshot.Publishers.Single(p => p.PublisherID == publisher.PublisherID);
        var dropGame = FindDroppableDraftedGame(publisherViewModel);
        var targetMasterGameID = await PickAvailableBidTargetAsync(
            league, publisher, [dropGame.MasterGame!.MasterGameID]);

        await LeaguePickupActions.PlaceDropAsync(publisher, dropGame.PublisherGameID);

        var result = await LeaguePickupActions.TryPlaceBidAsync(publisher, targetMasterGameID, 10, false);

        Assert.That(result.Success, Is.True);
    }

    [Test]
    public async Task DropThenBidThenCancelDrop_Blocked()
    {
        var league = await CreateFullRosterLeagueAsync();
        var publisher = league.Publishers[0];
        var snapshot = await publisher.Session.League.GetLeagueYearAsync(league.LeagueID, league.Year, null);
        var publisherViewModel = snapshot.Publishers.Single(p => p.PublisherID == publisher.PublisherID);
        var dropGame = FindDroppableDraftedGame(publisherViewModel);
        var targetMasterGameID = await PickAvailableBidTargetAsync(
            league, publisher, [dropGame.MasterGame!.MasterGameID]);

        await LeaguePickupActions.PlaceDropAsync(publisher, dropGame.PublisherGameID);
        await LeaguePickupActions.PlaceBidAsync(publisher, targetMasterGameID, 10, false);

        snapshot = await publisher.Session.League.GetLeagueYearAsync(league.LeagueID, league.Year, null);
        var dropRequestID = snapshot.PrivatePublisherData!.MyActiveDrops.Single().DropRequestID;

        var (success, error) = await LeaguePickupActions.TryDeleteDropAsync(publisher, dropRequestID);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.False);
            Assert.That(error, Does.Contain(BidSlotPathFunctions.DropRemovalBlockedError));
        }
    }

    [Test]
    public async Task EditBid_RemoveConditionalDropWhileFull_Rejected()
    {
        var league = await CreateFullRosterLeagueAsync();
        var publisher = league.Publishers[0];
        var snapshot = await publisher.Session.League.GetLeagueYearAsync(league.LeagueID, league.Year, null);
        var publisherViewModel = snapshot.Publishers.Single(p => p.PublisherID == publisher.PublisherID);
        var dropGame = FindDroppableDraftedGame(publisherViewModel);
        var targetMasterGameID = await PickAvailableBidTargetAsync(
            league, publisher, [dropGame.MasterGame!.MasterGameID]);

        await LeaguePickupActions.PlaceBidAsync(
            publisher, targetMasterGameID, 10, false, dropGame.PublisherGameID);

        snapshot = await publisher.Session.League.GetLeagueYearAsync(league.LeagueID, league.Year, null);
        var bidID = snapshot.PrivatePublisherData!.MyActiveBids.Single().BidID;

        var result = await LeaguePickupActions.TryEditPickupBidAsync(
            publisher, bidID, 10, conditionalDropPublisherGameID: null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Errors, Does.Contain(BidSlotPathFunctions.NoSlotPathError));
        }
    }

    [Test]
    public async Task CounterPickBidOnly_CancelDrop_Succeeds()
    {
        var league = await CreateFullStandardRosterOpenCounterPickSlotLeagueAsync();
        var publisher = league.Publishers[0];
        var snapshot = await publisher.Session.League.GetLeagueYearAsync(league.LeagueID, league.Year, null);
        var publisherViewModel = snapshot.Publishers.Single(p => p.PublisherID == publisher.PublisherID);
        var dropGame = FindDroppableDraftedGame(publisherViewModel);

        var possibleCounterPicks = await publisher.Session.League.PossibleCounterPicksAsync(publisher.PublisherID);
        var counterPickTargetID = possibleCounterPicks
            .First(g => g.MasterGame != null && !g.MasterGame.DelayContention && !g.MasterGame.IsReleased)
            .MasterGame!.MasterGameID;

        await LeaguePickupActions.PlaceBidAsync(publisher, counterPickTargetID, 10, counterPick: true);
        await LeaguePickupActions.PlaceDropAsync(publisher, dropGame.PublisherGameID);

        snapshot = await publisher.Session.League.GetLeagueYearAsync(league.LeagueID, league.Year, null);
        var dropRequestID = snapshot.PrivatePublisherData!.MyActiveDrops.Single().DropRequestID;

        var (success, error) = await LeaguePickupActions.TryDeleteDropAsync(publisher, dropRequestID);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(error, Is.Null);
        }
    }

    [Test]
    public async Task FullCounterPickSlots_CounterPickBid_Rejected()
    {
        var league = await CreateFullRosterLeagueAsync();
        var publisher = league.Publishers[0];

        var possibleCounterPicks = await publisher.Session.League.PossibleCounterPicksAsync(publisher.PublisherID);
        var counterPickTargetID = possibleCounterPicks
            .First(g => g.MasterGame != null && !g.MasterGame.DelayContention && !g.MasterGame.IsReleased)
            .MasterGame!.MasterGameID;

        var result = await LeaguePickupActions.TryPlaceBidAsync(
            publisher, counterPickTargetID, 10, counterPick: true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Errors, Does.Contain(BidSlotPathFunctions.NoSlotPathError));
        }
    }

    private async Task<LeagueFixture> CreateFullRosterLeagueAsync()
    {
        var league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, LeagueScenarios.FourPlayerDrops, NewUser);
        await league.DraftToCompletionAsync();
        _league = league;
        return league;
    }

    private async Task<LeagueFixture> CreateFullStandardRosterOpenCounterPickSlotLeagueAsync()
    {
        var scenario = new LeagueScenario
        {
            Name = "FullStandardOpenCounterPick",
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

        var league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, scenario, NewUser);
        await league.DraftToCompletionAsync();
        _league = league;
        return league;
    }

    private static PublisherGameViewModel FindDroppableDraftedGame(PublisherViewModel publisher)
    {
        return publisher.Games.First(g =>
            !g.CounterPick
            && !g.DropBlocked
            && g.MasterGame != null
            && g.OverallPickNumber.HasValue);
    }

    private static async Task<Guid> PickAvailableBidTargetAsync(
        LeagueFixture league,
        TestPublisher publisher,
        IEnumerable<Guid> excludedMasterGameIDs)
    {
        var excluded = excludedMasterGameIDs.ToHashSet();
        var available = await publisher.Session.League.TopAvailableGamesAsync(
            league.Year, league.LeagueID, publisher.PublisherID, null);
        var target = available.First(g =>
            g.IsAvailable
            && !g.Taken
            && !g.IsReleased
            && g.MasterGame != null
            && !excluded.Contains(g.MasterGame.MasterGameID));

        return target.MasterGame!.MasterGameID;
    }
}
