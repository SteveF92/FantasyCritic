using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Actions;

/// <summary>
/// Covers SemiPublicBiddingSemiPublicCounterPicks when counter picks ARE in play at the Thursday reveal. The league is told only
/// that something is being counter picked - never which games or how many - so that "yes" cannot be changed by anyone placing
/// another counter pick bid, and counter pick bidding stays open for the rest of the week. What it does forbid is cancelling a
/// counter pick bid placed before the reveal, since that could take the answer back to "no".
/// </summary>
[TestFixture]
public class PublicBiddingHiddenCounterPickTests : IntegrationTestBase
{
    private static readonly LeagueScenario HiddenCounterPickScenario = new()
    {
        Name = "HiddenCounterPicks",
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

    /// <summary>Monday, well before the Thursday reveal, so these bids count as pre-reveal.</summary>
    private static readonly DateTimeOffset BeforeReveal = new(2025, 1, 6, 12, 0, 0, TimeSpan.Zero);

    /// <summary>Thursday 2025-01-09 21:00 ET — one hour after that week's public bidding reveal.</summary>
    private static readonly DateTimeOffset AfterReveal = new(2025, 1, 10, 2, 0, 0, TimeSpan.Zero);

    private sealed record BidTargets(Guid StandardGame, Guid CounterPickA, Guid CounterPickB);

    private ApiSession _adminSession = null!;
    private LeagueFixture _league = null!;
    private BidTargets _targets = null!;
    private PublicBiddingSetViewModel _revealedSet = null!;

    [OneTimeSetUp]
    public async Task SetUpRevealedLeague()
    {
        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);
        await _adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest { NewTime = BeforeReveal });

        _league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, HiddenCounterPickScenario, NewUser);
        await _league.DraftToCompletionAsync();
        _league.CapturePublisherState(await _league.GetLeagueYearAsync());

        _targets = await PickBidTargetsAsync();

        await LeaguePickupActions.PlaceBidAsync(_league.Publishers[0], _targets.StandardGame, 10, false);
        await LeaguePickupActions.PlaceBidAsync(_league.Publishers[1], _targets.CounterPickA, 5, true);
        await LeaguePickupActions.PlaceBidAsync(_league.Publishers[2], _targets.CounterPickB, 5, true);

        await _adminSession.Admin.SetTimeAsync(new SetTimeRequest { NewTime = AfterReveal });

        // Publisher 4 placed no bids, so nothing it sees can come from its own knowledge of the week.
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
    public void Reveal_NamesTheStandardBidGameAndNothingElse()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_revealedSet.MasterGames, Has.Count.EqualTo(1),
                "Only the standard bid belongs in the list; counter picks are represented by the flag alone.");
            Assert.That(_revealedSet.MasterGames.Single().MasterGame.MasterGameID, Is.EqualTo(_targets.StandardGame));
            Assert.That(_revealedSet.MasterGames.Single().CounterPick, Is.False);
        }
    }

    [Test]
    public void Reveal_SaysSomethingIsBeingCounterPicked()
    {
        Assert.That(_revealedSet.AnyHiddenCounterPickBids, Is.True,
            "Two games are being counter picked, so the league should be told that counter picking is happening.");
    }

    [Test]
    public void Reveal_DoesNotMentionEitherCounterPickedGameAnywhere()
    {
        var revealedGameIDs = _revealedSet.MasterGames.Select(x => x.MasterGame.MasterGameID).ToList();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(revealedGameIDs, Does.Not.Contain(_targets.CounterPickA),
                "The first counter picked game leaked into the revealed list.");
            Assert.That(revealedGameIDs, Does.Not.Contain(_targets.CounterPickB),
                "The second counter picked game leaked into the revealed list.");
        }
    }

    [Test]
    public async Task Reveal_ThisWeeksPublicBiddingGames_OmitsCounterPickedGames()
    {
        var publisher = _league.Publishers[3];
        var biddableGames = await publisher.Session.League.ThisWeeksPublicBiddingGamesAsync(
            _league.Year, _league.LeagueID, publisher.PublisherID);

        var biddableGameIDs = biddableGames.Select(x => x.MasterGame.MasterGameID).ToList();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(biddableGameIDs, Does.Not.Contain(_targets.CounterPickA));
            Assert.That(biddableGameIDs, Does.Not.Contain(_targets.CounterPickB));
        }
    }

    [Test]
    public async Task AfterReveal_CounterPickBidPlacedBeforeRevealCannotBeCancelled()
    {
        var publisher = _league.Publishers[1];
        var snapshot = await publisher.Session.League.GetLeagueYearAsync(_league.LeagueID, _league.Year, null);
        var counterPickBid = snapshot.PrivatePublisherData!.MyActiveBids.Single(x => x.CounterPick);

        ApiException? ex = null;
        try
        {
            await publisher.Session.League.DeletePickupBidAsync(new PickupBidDeleteRequest
            {
                PublisherID = publisher.PublisherID,
                BidID = counterPickBid.BidID,
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null,
            "Cancelling could take the league's answer back to \"nothing is being counter picked\".");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }

    /// <summary>
    /// The whole point of revealing a yes/no rather than a count: joining in is still allowed, and backing out of a bid you placed
    /// after the reveal is too, because neither can change an answer that is already "yes".
    /// </summary>
    [Test]
    public async Task AfterReveal_NewCounterPickBidIsAllowedAndCanBeCancelled()
    {
        var publisher = _league.Publishers[3];
        var possibleCounterPicks = await publisher.Session.League.PossibleCounterPicksAsync(publisher.PublisherID);
        var target = possibleCounterPicks
            .First(g => g.MasterGame is not null && !g.MasterGame.DelayContention && !g.MasterGame.IsReleased)
            .MasterGame!.MasterGameID;

        var result = await LeaguePickupActions.TryPlaceBidAsync(publisher, target, 5, counterPick: true);
        Assert.That(result.Success, Is.True,
            () => $"Counter pick bidding stays open while something is already being counter picked. Errors: {string.Join("; ", result.Errors ?? [])}");

        var snapshot = await publisher.Session.League.GetLeagueYearAsync(_league.LeagueID, _league.Year, null);
        var placedBid = snapshot.PrivatePublisherData!.MyActiveBids.Single(x => x.CounterPick);

        await publisher.Session.League.DeletePickupBidAsync(new PickupBidDeleteRequest
        {
            PublisherID = publisher.PublisherID,
            BidID = placedBid.BidID,
        });

        var afterSnapshot = await publisher.Session.League.GetLeagueYearAsync(_league.LeagueID, _league.Year, null);
        Assert.That(afterSnapshot.PrivatePublisherData!.MyActiveBids.Any(x => x.CounterPick), Is.False,
            "A bid placed after the reveal can be withdrawn, since the earlier bids still keep the answer at \"yes\".");
    }

    [Test]
    public async Task AfterReveal_StandardBidOnARevealedGameIsStillAllowed()
    {
        var result = await LeaguePickupActions.TryPlaceBidAsync(
            _league.Publishers[3], _targets.StandardGame, 7, counterPick: false);

        Assert.That(result.Success, Is.True,
            () => $"Standard bidding on an already-revealed game should still work. Errors: {string.Join("; ", result.Errors ?? [])}");
    }

    private async Task<BidTargets> PickBidTargetsAsync()
    {
        var available = await _league.Publishers[0].Session.League.TopAvailableGamesAsync(
            _league.Year, _league.LeagueID, _league.Publishers[0].PublisherID, null);
        var standardGame = available.First(g => g.IsAvailable && !g.Taken && !g.IsReleased).MasterGame.MasterGameID;

        var counterPickA = await FirstCounterPickTargetAsync(_league.Publishers[1], []);
        var counterPickB = await FirstCounterPickTargetAsync(_league.Publishers[2], [counterPickA]);

        return new BidTargets(standardGame, counterPickA, counterPickB);
    }

    private static async Task<Guid> FirstCounterPickTargetAsync(TestPublisher publisher, IReadOnlyCollection<Guid> exclude)
    {
        var possibleCounterPicks = await publisher.Session.League.PossibleCounterPicksAsync(publisher.PublisherID);
        return possibleCounterPicks
            .Where(g => g.MasterGame is not null && !g.MasterGame.DelayContention && !g.MasterGame.IsReleased)
            .Select(g => g.MasterGame!.MasterGameID)
            .First(id => !exclude.Contains(id));
    }
}
