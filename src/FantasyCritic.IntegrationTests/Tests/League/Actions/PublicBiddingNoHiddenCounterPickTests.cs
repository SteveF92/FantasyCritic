using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Actions;

/// <summary>
/// Covers SemiPublicBiddingSemiPublicCounterPicks when NOTHING is being counter picked at the Thursday reveal. The league has been
/// told that, so any counter pick bid at all would change the answer from "no" to "yes" - and letting a player discover that by
/// submitting one and seeing whether it took would give the same thing away. Counter pick bidding is therefore closed outright.
/// </summary>
[TestFixture]
public class PublicBiddingNoHiddenCounterPickTests : IntegrationTestBase
{
    private static readonly LeagueScenario NoHiddenCounterPickScenario = new()
    {
        Name = "NoHiddenCounterPicks",
        PlayerCount = 4,
        StandardGames = 6,
        GamesToDraft = 3,
        CounterPicks = 2,
        CounterPicksToDraft = 1,
        PickupSystem = "SemiPublicBiddingSemiPublicCounterPicks",
        ScoringSystem = "LinearPositive",
        TradingSystem = "Standard",
        TiebreakSystem = "LowestProjectedPoints",
        ReleaseSystem = "MustBeReleased",
        IneligibleGameSystem = "DroppableAsWillNotRelease",
        MinimumBidAmount = 0,
        EnableBids = true,
    };

    private static readonly DateTimeOffset BeforeReveal = new(2025, 1, 6, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset AfterReveal = new(2025, 1, 10, 2, 0, 0, TimeSpan.Zero);

    private ApiSession _adminSession = null!;
    private LeagueFixture _league = null!;
    private Guid _standardGame;
    private PublicBiddingSetViewModel _revealedSet = null!;

    [OneTimeSetUp]
    public async Task SetUpRevealedLeague()
    {
        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);
        await _adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest { NewTime = BeforeReveal });

        _league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, NoHiddenCounterPickScenario, NewUser);
        await _league.DraftToCompletionAsync();
        _league.CapturePublisherState(await _league.GetLeagueYearAsync());

        var available = await _league.Publishers[0].Session.League.TopAvailableGamesAsync(
            _league.Year, _league.LeagueID, _league.Publishers[0].PublisherID, null);
        _standardGame = available.First(g => g.IsAvailable && !g.Taken && !g.IsReleased).MasterGame.MasterGameID;

        // A standard bid only - deliberately no counter pick bids this week.
        await LeaguePickupActions.PlaceBidAsync(_league.Publishers[0], _standardGame, 10, false);

        await _adminSession.Admin.SetTimeAsync(new SetTimeRequest { NewTime = AfterReveal });

        var snapshot = await _league.Publishers[3].Session.League.GetLeagueYearAsync(_league.LeagueID, _league.Year, null);
        _revealedSet = snapshot.PublicBiddingGames
            ?? throw new InvalidOperationException("Expected the league to be inside the public bidding window.");
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        if (_adminSession is not null)
        {
            await _adminSession.Admin.ResetTimeAsync();
            _adminSession.Dispose();
        }

        if (_league is not null)
        {
            await _league.DisposeAsync();
        }
    }

    [Test]
    public void Reveal_SaysNothingIsBeingCounterPicked()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_revealedSet.AnyHiddenCounterPickBids, Is.False);
            Assert.That(_revealedSet.MasterGames.Select(x => x.MasterGame.MasterGameID), Is.EquivalentTo(new[] { _standardGame }));
        }
    }

    [Test]
    public async Task AfterReveal_CounterPickBidIsRejected()
    {
        var publisher = _league.Publishers[3];
        var possibleCounterPicks = await publisher.Session.League.PossibleCounterPicksAsync(publisher.PublisherID);
        var target = possibleCounterPicks
            .First(g => g.MasterGame is not null && !g.MasterGame.DelayContention && !g.MasterGame.IsReleased)
            .MasterGame!.MasterGameID;

        ApiException? ex = null;
        try
        {
            await LeaguePickupActions.TryPlaceBidAsync(publisher, target, 5, counterPick: true);
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null,
            "The league was told nothing is being counter picked, and this bid would change that answer.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task AfterReveal_StandardBidOnARevealedGameIsStillAllowed()
    {
        var result = await LeaguePickupActions.TryPlaceBidAsync(
            _league.Publishers[3], _standardGame, 7, counterPick: false);

        Assert.That(result.Success, Is.True,
            () => $"Standard bidding on an already-revealed game should still work. Errors: {string.Join("; ", result.Errors ?? [])}");
    }
}
