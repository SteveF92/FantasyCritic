using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Actions;

/// <summary>
/// Guards the SemiPublicBiddingSecretCounterPicks system, which reveals nothing whatsoever about counter picks. Because the
/// reveal says nothing about them, counter pick bids stay completely unconstrained after Thursday — they can still be placed and
/// cancelled — which is what separates this system from SemiPublicBiddingSemiPublicCounterPicks.
/// </summary>
[TestFixture]
public class PublicBiddingSecretCounterPickTests : IntegrationTestBase
{
    private static readonly LeagueScenario SecretCounterPickScenario = new()
    {
        Name = "SecretCounterPicks",
        PlayerCount = 4,
        StandardGames = 6,
        GamesToDraft = 3,
        CounterPicks = 2,
        CounterPicksToDraft = 1,
        PickupSystem = "SemiPublicBiddingSecretCounterPicks",
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
    private Guid _counterPickTarget;

    [OneTimeSetUp]
    public async Task SetUpRevealedLeague()
    {
        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);
        await _adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest { NewTime = BeforeReveal });

        _league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, SecretCounterPickScenario, NewUser);
        await _league.DraftToCompletionAsync();
        _league.CapturePublisherState(await _league.GetLeagueYearAsync());

        var available = await _league.Publishers[0].Session.League.TopAvailableGamesAsync(
            _league.Year, _league.LeagueID, _league.Publishers[0].PublisherID, null);
        _standardGame = available.First(g => g.IsAvailable && !g.Taken && !g.IsReleased).MasterGame.MasterGameID;

        var possibleCounterPicks = await _league.Publishers[1].Session.League.PossibleCounterPicksAsync(
            _league.Publishers[1].PublisherID);
        _counterPickTarget = possibleCounterPicks
            .First(g => g.MasterGame is not null && !g.MasterGame.DelayContention && !g.MasterGame.IsReleased)
            .MasterGame!.MasterGameID;

        await LeaguePickupActions.PlaceBidAsync(_league.Publishers[0], _standardGame, 10, false);
        await LeaguePickupActions.PlaceBidAsync(_league.Publishers[1], _counterPickTarget, 5, true);

        await _adminSession.Admin.SetTimeAsync(new SetTimeRequest { NewTime = AfterReveal });
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
    public async Task Reveal_ShowsStandardBidsOnlyAndNoCounterPickTraceAtAll()
    {
        var snapshot = await _league.Publishers[3].Session.League.GetLeagueYearAsync(_league.LeagueID, _league.Year, null);
        var revealedSet = snapshot.PublicBiddingGames
            ?? throw new InvalidOperationException("Expected the league to be inside the public bidding window.");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(revealedSet.MasterGames.Any(x => x.CounterPick), Is.False,
                "Secret counter picks leave no trace in the revealed list.");
            Assert.That(revealedSet.AnyHiddenCounterPickBids, Is.False,
                "Even the yes/no counter pick signal belongs to the semi-public system, not this one.");
            Assert.That(revealedSet.MasterGames.Select(x => x.MasterGame!.MasterGameID), Is.EquivalentTo(new[] { _standardGame }));
        }
    }

    [Test]
    public async Task AfterReveal_NewCounterPickBidIsStillAllowed()
    {
        var publisher = _league.Publishers[2];
        var possibleCounterPicks = await publisher.Session.League.PossibleCounterPicksAsync(publisher.PublisherID);
        var target = possibleCounterPicks
            .First(g => g.MasterGame is not null && !g.MasterGame.DelayContention && !g.MasterGame.IsReleased)
            .MasterGame!.MasterGameID;

        var result = await LeaguePickupActions.TryPlaceBidAsync(publisher, target, 5, counterPick: true);

        Assert.That(result.Success, Is.True,
            () => $"The reveal says nothing about counter picks, so placing one stays legal. Errors: {string.Join("; ", result.Errors ?? [])}");
    }

    [Test]
    public async Task AfterReveal_CounterPickBidCanStillBeCancelled()
    {
        var publisher = _league.Publishers[1];
        var snapshot = await publisher.Session.League.GetLeagueYearAsync(_league.LeagueID, _league.Year, null);
        var counterPickBid = snapshot.PrivatePublisherData!.MyActiveBids.Single(x => x.CounterPick);

        await publisher.Session.League.DeletePickupBidAsync(new PickupBidDeleteRequest
        {
            PublisherID = publisher.PublisherID,
            BidID = counterPickBid.BidID,
        });

        var afterSnapshot = await publisher.Session.League.GetLeagueYearAsync(_league.LeagueID, _league.Year, null);
        Assert.That(afterSnapshot.PrivatePublisherData!.MyActiveBids.Any(x => x.CounterPick), Is.False,
            "The reveal says nothing about counter picks, so cancelling one changes nothing the league was shown.");
    }

    [Test]
    public async Task AfterReveal_StandardBidPlacedBeforeRevealCannotBeCancelled()
    {
        var publisher = _league.Publishers[0];
        var snapshot = await publisher.Session.League.GetLeagueYearAsync(_league.LeagueID, _league.Year, null);
        var standardBid = snapshot.PrivatePublisherData!.MyActiveBids.Single(x => !x.CounterPick);

        ApiException? ex = null;
        try
        {
            await publisher.Session.League.DeletePickupBidAsync(new PickupBidDeleteRequest
            {
                PublisherID = publisher.PublisherID,
                BidID = standardBid.BidID,
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null, "The standard bid is already public, so cancelling it would change what the league saw.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }
}
