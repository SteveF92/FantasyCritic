using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Royale;

/// <summary>
/// Integration tests asserting Q2 2026 still uses its locked-in rules while Q3 changes are gated away:
///   - 5-day buy/sell/advertising lockout (not 7)
///   - No bidding-cycle purchase restriction
///
/// The clock is pinned to June 15, 2026 12:00 UTC — mid Q2.
/// Requires Q2 2026 to be open for play in the local database.
/// </summary>
[TestFixture]
public class RoyaleQ22026Tests : IntegrationTestBase
{
    private const string BiddingCycleStatus = "Game will become eligible after bids process this week.";

    private static readonly DateTimeOffset Q2TestBase =
        new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    private ApiSession _adminSession = null!;

    [OneTimeSetUp]
    public async Task SetUpQ2Clock()
    {
        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);
        await _adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest { NewTime = Q2TestBase });
    }

    [OneTimeTearDown]
    public async Task TearDownQ2Clock()
    {
        if (_adminSession is not null)
        {
            await _adminSession.Admin.ResetTimeAsync();
            _adminSession.Dispose();
        }
    }

    private Task ResetClockAsync() =>
        _adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest { NewTime = Q2TestBase });

    private async Task<(ApiSession Session, Guid PublisherID)?> TryCreateQ2PublisherAsync()
    {
        var (email, password, displayName) = NewUser();
        var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        try
        {
            var publisherID = await session.Royale.CreateRoyalePublisherAsync(
                new CreateRoyalePublisherRequest
                {
                    Year = 2026,
                    Quarter = 2,
                    PublisherName = $"Pub-{Guid.NewGuid():N}"[..20],
                });
            return (session, publisherID);
        }
        catch (ApiException)
        {
            session.Dispose();
            return null;
        }
    }

    [Test]
    public async Task PossibleGames_InQ22026_DoesNotApplyBiddingCycleRule()
    {
        await ResetClockAsync();

        var result = await TryCreateQ2PublisherAsync();
        if (result is null)
        {
            Assert.Inconclusive("Q2 2026 is not open for play. Run LocalDatabaseTool to sync.");
            return;
        }

        using var session = result.Value.Session;
        var publisherID = result.Value.PublisherID;

        var allGames = await session.Royale.PossibleMasterGamesAsync(
            gameName: null, publisherID: publisherID,
            releaseFilter: RoyalePossibleMasterGamesReleaseFilter.All);

        var biddingCycleBlocked = (allGames ?? [])
            .Where(g => g.Status == BiddingCycleStatus)
            .ToList();

        Assert.That(
            biddingCycleBlocked,
            Is.Empty,
            "The bidding-cycle restriction is Q3+ only — Q2 possible games must never show that status.");
    }

    [Test]
    public async Task PurchaseGame_InQ22026_5DayLockoutWindow_ReturnsFailure()
    {
        await ResetClockAsync();

        var result = await TryCreateQ2PublisherAsync();
        if (result is null)
        {
            Assert.Inconclusive("Q2 2026 is not open for play. Run LocalDatabaseTool to sync.");
            return;
        }

        using var session = result.Value.Session;
        var publisherID = result.Value.PublisherID;

        var allGames = await session.Royale.PossibleMasterGamesAsync(
            gameName: null, publisherID: publisherID,
            releaseFilter: RoyalePossibleMasterGamesReleaseFilter.All);

        const string lockoutStatus = "Game will release within 5 days.";
        var lockoutGame = (allGames ?? [])
            .FirstOrDefault(g => g.IsAvailable != true && g.Status == lockoutStatus);

        if (lockoutGame is null)
        {
            Assert.Inconclusive(
                $"No game has Status == \"{lockoutStatus}\". " +
                "This test only runs when a game is releasing within 5 days of June 15, 2026. Skipping.");
            return;
        }

        var purchaseResult = await session.Royale.PurchaseGameAsync(
            new PurchaseRoyaleGameRequest
            {
                PublisherID = publisherID,
                MasterGameID = lockoutGame.MasterGame.MasterGameID,
            });

        Assert.That(purchaseResult!.Success, Is.False,
            "Purchasing a game inside the 5-day Q2 lockout window must return Success=false.");
        Assert.That(
            purchaseResult.Errors!.Any(e => e.Contains("5")),
            Is.True,
            "The failure message must reference the 5-day lockout window.");
    }

    [Test]
    public async Task PossibleGames_InQ22026_AvailableGamesMayCostBelowThreeDollars()
    {
        // Q2 keeps the $2 minimum and 1.5× multiplier — unlike Q3's $3 floor with no multiplier.
        await ResetClockAsync();

        var result = await TryCreateQ2PublisherAsync();
        if (result is null)
        {
            Assert.Inconclusive("Q2 2026 is not open for play. Run LocalDatabaseTool to sync.");
            return;
        }

        using var session = result.Value.Session;
        var publisherID = result.Value.PublisherID;

        var games = await session.Royale.PossibleMasterGamesAsync(
            gameName: null, publisherID: publisherID,
            releaseFilter: RoyalePossibleMasterGamesReleaseFilter.All);

        var available = (games ?? []).Where(g => g.IsAvailable == true).ToList();
        if (available.Count == 0)
        {
            Assert.Inconclusive("No purchasable games found for Q2 2026 — cannot verify pricing rules.");
            return;
        }

        Assert.That(
            available.Any(g => g.Cost < 3m),
            Is.True,
            "Q2 2026 should still allow sub-$3 game prices (minimum $2 with 1.5× multiplier).");
    }
}
