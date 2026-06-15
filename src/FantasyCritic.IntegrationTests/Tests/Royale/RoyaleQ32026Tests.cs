using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using FantasyCritic.IntegrationTests.ProductionStats;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Royale;

/// <summary>
/// Integration tests for the Q3 2026 Royale rule changes:
///   - Game prices have no 1.5× multiplier and a $3 minimum (up from $2)
///   - Selling a game refunds half of the purchase price (instead of half market value)
///   - A 10-minute regret window allows a full refund immediately after buying
///   - The buy/sell/advertising lockout window is 7 days (up from 5)
///   - Games added since the last bid cycle cannot be purchased until bids process
///
/// The clock is pinned to July 6, 2026 12:00 UTC via the test-only Admin API —
/// safely after the first Q3 bid cycle (July 4, 20:00 ET = July 5, 00:00 UTC).
/// Individual tests advance the clock with SetTimeAsync when time-sensitive
/// behaviour (e.g. regret-window expiry) needs to be verified.
///
/// All tests require Q3 2026 to be open for play in the local database.
/// If it is not, the test is marked Inconclusive — run LocalDatabaseTool to sync.
/// </summary>
[TestFixture]
public class RoyaleQ32026Tests : IntegrationTestBase
{
    private const string BiddingCycleStatus = "Game will become eligible after bids process this week.";

    // Q3 2026 starts July 1. First Saturday bid processing: July 4, 20:00 ET = July 5, 00:00 UTC.
    // We pin to July 6 12:00 UTC — after the first bid cycle, safely in mid-Q3.
    private static readonly DateTimeOffset Q3TestBase =
        new DateTimeOffset(2026, 7, 6, 12, 0, 0, TimeSpan.Zero);

    private ApiSession _adminSession = null!;

    [OneTimeSetUp]
    public async Task SetUpQ3Clock()
    {
        _adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(_adminSession);
        await _adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest { NewTime = Q3TestBase });
    }

    [OneTimeTearDown]
    public async Task TearDownQ3Clock()
    {
        if (_adminSession is not null)
        {
            await _adminSession.Admin.ResetTimeAsync();
            _adminSession.Dispose();
        }
    }

    // ── Clock helpers ─────────────────────────────────────────────────────────

    private Task ResetClockAsync() =>
        _adminSession.Admin.SetTimeAsync(new SetTimeRequest { NewTime = Q3TestBase });

    private Task AdvanceClockByAsync(TimeSpan offset) =>
        _adminSession.Admin.SetTimeAsync(new SetTimeRequest { NewTime = Q3TestBase + offset });

    // ── Test-publisher factory ────────────────────────────────────────────────

    /// <summary>
    /// Creates a new user and a publisher in Q3 2026.
    /// Returns <c>null</c> if Q3 2026 is not open for play (caller should mark inconclusive).
    /// The caller is responsible for disposing the returned <see cref="ApiSession"/>.
    /// </summary>
    private async Task<(ApiSession Session, Guid PublisherID)?> TryCreateQ3PublisherAsync()
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
                    Quarter = 3,
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

    // ── Game-finder helper ────────────────────────────────────────────────────

    private static async Task<PossibleRoyaleMasterGameViewModel?> FindPurchasableQ3GameAsync(
        ApiSession session, Guid publisherID)
    {
        foreach (var filter in new[]
                 {
                     RoyalePossibleMasterGamesReleaseFilter.ExpectedToReleaseInQuarter,
                     RoyalePossibleMasterGamesReleaseFilter.All,
                 })
        {
            var games = await session.Royale.PossibleMasterGamesAsync(
                gameName: null, publisherID: publisherID, releaseFilter: filter);

            var available = (games ?? []).Where(g => g.IsAvailable == true).ToList();
            if (available.Count > 0)
            {
                return await ProductionGameStatsCache.FindHighestHypeAvailableAsync(
                    available,
                    g => g.MasterGame.MasterGameID,
                    2026);
            }
        }

        return null;
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Test]
    public async Task PossibleGames_InQ32026_AllAvailableGamesCostAtLeastThreeDollars()
    {
        await ResetClockAsync();

        var result = await TryCreateQ3PublisherAsync();
        if (result is null)
        {
            Assert.Inconclusive("Q3 2026 is not open for play. Run LocalDatabaseTool to sync.");
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
            Assert.Inconclusive("No purchasable games found for Q3 2026 — cannot verify minimum price.");
            return;
        }

        Assert.That(
            available.All(g => g.Cost >= 3m),
            Is.True,
            "Every available Q3 2026 game must cost at least $3 (minimum price raised from $2, 1.5× multiplier removed).");
    }

    [Test]
    public async Task SellGame_ImmediatelyAfterPurchase_RefundsFullAmountSpent()
    {
        // Within the 10-minute regret window a full refund of AmountSpent is expected.
        await ResetClockAsync();

        var result = await TryCreateQ3PublisherAsync();
        if (result is null)
        {
            Assert.Inconclusive("Q3 2026 is not open for play. Run LocalDatabaseTool to sync.");
            return;
        }

        using var session = result.Value.Session;
        var publisherID = result.Value.PublisherID;

        var game = await FindPurchasableQ3GameAsync(session, publisherID);
        if (game is null)
        {
            Assert.Inconclusive("No purchasable Q3 2026 games found.");
            return;
        }

        var purchaseResult = await session.Royale.PurchaseGameAsync(
            new PurchaseRoyaleGameRequest
            {
                PublisherID = publisherID,
                MasterGameID = game.MasterGame.MasterGameID,
            });
        Assert.That(purchaseResult!.Success, Is.True,
            $"Setup purchase failed: {string.Join("; ", purchaseResult.Errors ?? [])}");

        // Fetch immediately — the clock has not moved; we are still within the 10-minute window.
        var publisher = await session.Royale.GetRoyalePublisherAsync(publisherID);
        var purchased = publisher!.PublisherGames!
            .Single(g => g.MasterGame?.MasterGameID == game.MasterGame.MasterGameID);

        // Ineligible games also give a full refund, so rule that out to isolate the regret window.
        Assert.That(purchased.CurrentlyIneligible, Is.False,
            "Game should not be ineligible — we want to verify the regret-window path, not the ineligibility path.");

        Assert.That(purchased.RefundAmount, Is.EqualTo(purchased.AmountSpent),
            "Selling within the 10-minute regret window must refund the full purchase price.");
    }

    [Test]
    public async Task SellGame_AfterRegretWindowExpires_RefundsHalfOfAmountSpent()
    {
        await ResetClockAsync();

        var result = await TryCreateQ3PublisherAsync();
        if (result is null)
        {
            Assert.Inconclusive("Q3 2026 is not open for play. Run LocalDatabaseTool to sync.");
            return;
        }

        using var session = result.Value.Session;
        var publisherID = result.Value.PublisherID;

        var game = await FindPurchasableQ3GameAsync(session, publisherID);
        if (game is null)
        {
            Assert.Inconclusive("No purchasable Q3 2026 games found.");
            return;
        }

        var purchaseResult = await session.Royale.PurchaseGameAsync(
            new PurchaseRoyaleGameRequest
            {
                PublisherID = publisherID,
                MasterGameID = game.MasterGame.MasterGameID,
            });
        Assert.That(purchaseResult!.Success, Is.True,
            $"Setup purchase failed: {string.Join("; ", purchaseResult.Errors ?? [])}");

        // Advance the clock 15 minutes past the purchase timestamp — regret window (10 min) has expired.
        await AdvanceClockByAsync(TimeSpan.FromMinutes(15));

        var publisher = await session.Royale.GetRoyalePublisherAsync(publisherID);
        var purchased = publisher!.PublisherGames!
            .Single(g => g.MasterGame?.MasterGameID == game.MasterGame.MasterGameID);

        Assert.That(purchased.CurrentlyIneligible, Is.False,
            "Game should not be ineligible — we want to verify the normal Q3 refund path.");

        Assert.That(purchased.RefundAmount, Is.EqualTo(purchased.AmountSpent * 0.5m),
            "After the regret window expires, the Q3 2026 refund must be exactly half the original purchase price.");
    }

    [Test]
    public async Task SellGame_AfterRegretWindow_BudgetIncreasesBy_HalfAmountSpentPlusAdvertising()
    {
        // End-to-end sell test: budget delta matches the refund formula.
        await ResetClockAsync();

        var result = await TryCreateQ3PublisherAsync();
        if (result is null)
        {
            Assert.Inconclusive("Q3 2026 is not open for play. Run LocalDatabaseTool to sync.");
            return;
        }

        using var session = result.Value.Session;
        var publisherID = result.Value.PublisherID;

        var game = await FindPurchasableQ3GameAsync(session, publisherID);
        if (game is null)
        {
            Assert.Inconclusive("No purchasable Q3 2026 games found.");
            return;
        }

        var purchaseResult = await session.Royale.PurchaseGameAsync(
            new PurchaseRoyaleGameRequest
            {
                PublisherID = publisherID,
                MasterGameID = game.MasterGame.MasterGameID,
            });
        Assert.That(purchaseResult!.Success, Is.True,
            $"Setup purchase failed: {string.Join("; ", purchaseResult.Errors ?? [])}");

        var publisherAfterBuy = await session.Royale.GetRoyalePublisherAsync(publisherID);
        var purchased = publisherAfterBuy!.PublisherGames!
            .Single(g => g.MasterGame?.MasterGameID == game.MasterGame.MasterGameID);

        var amountSpent = purchased.AmountSpent!.Value;
        var advertisingMoney = purchased.AdvertisingMoney;
        var budgetAfterBuy = publisherAfterBuy.Budget;

        // Advance past regret window, then sell.
        await AdvanceClockByAsync(TimeSpan.FromMinutes(15));

        await session.Royale.SellGameAsync(
            new SellRoyaleGameRequest
            {
                PublisherID = publisherID,
                MasterGameID = game.MasterGame.MasterGameID,
            });

        var publisherAfterSell = await session.Royale.GetRoyalePublisherAsync(publisherID);

        var expectedRefund = amountSpent * 0.5m + advertisingMoney;
        Assert.That(
            publisherAfterSell!.Budget,
            Is.EqualTo(budgetAfterBuy + expectedRefund).Within(0.001m),
            "Budget after sell must equal (budget before sell) + (half amount spent) + (advertising money).");

        Assert.That(
            publisherAfterSell.PublisherGames!.All(g => g.MasterGame?.MasterGameID != game.MasterGame.MasterGameID),
            Is.True,
            "Sold game must no longer appear on the publisher's roster.");
    }

    [Test]
    public async Task SellGame_InRegretWindow_BudgetIncreasesBy_FullAmountSpentPlusAdvertising()
    {
        // Regret-window sell returns the full price paid, not half market value.
        await ResetClockAsync();

        var result = await TryCreateQ3PublisherAsync();
        if (result is null)
        {
            Assert.Inconclusive("Q3 2026 is not open for play. Run LocalDatabaseTool to sync.");
            return;
        }

        using var session = result.Value.Session;
        var publisherID = result.Value.PublisherID;

        var game = await FindPurchasableQ3GameAsync(session, publisherID);
        if (game is null)
        {
            Assert.Inconclusive("No purchasable Q3 2026 games found.");
            return;
        }

        var publisherBeforeBuy = await session.Royale.GetRoyalePublisherAsync(publisherID);
        var budgetBeforeBuy = publisherBeforeBuy!.Budget;

        var purchaseResult = await session.Royale.PurchaseGameAsync(
            new PurchaseRoyaleGameRequest
            {
                PublisherID = publisherID,
                MasterGameID = game.MasterGame.MasterGameID,
            });
        Assert.That(purchaseResult!.Success, Is.True,
            $"Setup purchase failed: {string.Join("; ", purchaseResult.Errors ?? [])}");

        var publisherAfterBuy = await session.Royale.GetRoyalePublisherAsync(publisherID);
        var purchased = publisherAfterBuy!.PublisherGames!
            .Single(g => g.MasterGame?.MasterGameID == game.MasterGame.MasterGameID);
        var amountSpent = purchased.AmountSpent!.Value;
        var budgetAfterBuy = publisherAfterBuy.Budget;

        Assert.That(budgetAfterBuy, Is.EqualTo(budgetBeforeBuy - amountSpent).Within(0.001m),
            "Budget after purchase must be reduced by the game's cost.");

        // Sell immediately — still within the regret window.
        await session.Royale.SellGameAsync(
            new SellRoyaleGameRequest
            {
                PublisherID = publisherID,
                MasterGameID = game.MasterGame.MasterGameID,
            });

        var publisherAfterSell = await session.Royale.GetRoyalePublisherAsync(publisherID);

        Assert.That(
            publisherAfterSell!.Budget,
            Is.EqualTo(budgetBeforeBuy).Within(0.001m),
            "Selling within the regret window must restore the budget to its pre-purchase value.");
    }

    [Test]
    public async Task PurchaseGame_InQ32026_7DayLockoutWindow_ReturnsFailure()
    {
        await ResetClockAsync();

        var result = await TryCreateQ3PublisherAsync();
        if (result is null)
        {
            Assert.Inconclusive("Q3 2026 is not open for play. Run LocalDatabaseTool to sync.");
            return;
        }

        using var session = result.Value.Session;
        var publisherID = result.Value.PublisherID;

        var allGames = await session.Royale.PossibleMasterGamesAsync(
            gameName: null, publisherID: publisherID,
            releaseFilter: RoyalePossibleMasterGamesReleaseFilter.All);

        const string lockoutStatus = "Game will release within 7 days.";
        var lockoutGame = (allGames ?? [])
            .FirstOrDefault(g => g.IsAvailable != true && g.Status == lockoutStatus);

        if (lockoutGame is null)
        {
            Assert.Inconclusive(
                $"No game has Status == \"{lockoutStatus}\". " +
                "This test only runs when a game is releasing within 7 days of July 6, 2026. Skipping.");
            return;
        }

        var purchaseResult = await session.Royale.PurchaseGameAsync(
            new PurchaseRoyaleGameRequest
            {
                PublisherID = publisherID,
                MasterGameID = lockoutGame.MasterGame.MasterGameID,
            });

        Assert.That(purchaseResult!.Success, Is.False,
            "Purchasing a game inside the 7-day Q3 lockout window must return Success=false.");
        Assert.That(
            purchaseResult.Errors!.Any(e => e.Contains("7")),
            Is.True,
            "The failure message must reference the 7-day lockout window.");
    }

    [Test]
    public async Task InRegretWindow_IsTrueImmediatelyAfterPurchase()
    {
        await ResetClockAsync();

        var result = await TryCreateQ3PublisherAsync();
        if (result is null)
        {
            Assert.Inconclusive("Q3 2026 is not open for play. Run LocalDatabaseTool to sync.");
            return;
        }

        using var session = result.Value.Session;
        var publisherID = result.Value.PublisherID;

        var game = await FindPurchasableQ3GameAsync(session, publisherID);
        if (game is null)
        {
            Assert.Inconclusive("No purchasable Q3 2026 games found.");
            return;
        }

        var purchaseResult = await session.Royale.PurchaseGameAsync(
            new PurchaseRoyaleGameRequest
            {
                PublisherID = publisherID,
                MasterGameID = game.MasterGame.MasterGameID,
            });
        Assert.That(purchaseResult!.Success, Is.True,
            $"Setup purchase failed: {string.Join("; ", purchaseResult.Errors ?? [])}");

        var publisher = await session.Royale.GetRoyalePublisherAsync(publisherID);
        var purchased = publisher!.PublisherGames!
            .Single(g => g.MasterGame?.MasterGameID == game.MasterGame.MasterGameID);

        Assert.That(purchased.InRegretWindow, Is.True,
            "InRegretWindow must be true immediately after purchase.");
    }

    [Test]
    public async Task InRegretWindow_IsFalseAfterRegretWindowExpires()
    {
        await ResetClockAsync();

        var result = await TryCreateQ3PublisherAsync();
        if (result is null)
        {
            Assert.Inconclusive("Q3 2026 is not open for play. Run LocalDatabaseTool to sync.");
            return;
        }

        using var session = result.Value.Session;
        var publisherID = result.Value.PublisherID;

        var game = await FindPurchasableQ3GameAsync(session, publisherID);
        if (game is null)
        {
            Assert.Inconclusive("No purchasable Q3 2026 games found.");
            return;
        }

        var purchaseResult = await session.Royale.PurchaseGameAsync(
            new PurchaseRoyaleGameRequest
            {
                PublisherID = publisherID,
                MasterGameID = game.MasterGame.MasterGameID,
            });
        Assert.That(purchaseResult!.Success, Is.True,
            $"Setup purchase failed: {string.Join("; ", purchaseResult.Errors ?? [])}");

        await AdvanceClockByAsync(TimeSpan.FromMinutes(15));

        var publisher = await session.Royale.GetRoyalePublisherAsync(publisherID);
        var purchased = publisher!.PublisherGames!
            .Single(g => g.MasterGame?.MasterGameID == game.MasterGame.MasterGameID);

        Assert.That(purchased.InRegretWindow, Is.False,
            "InRegretWindow must be false once the 10-minute window has expired.");
    }

    [Test]
    public async Task PossibleGames_BiddingCycleBlockedGamesAreNotAvailable()
    {
        await ResetClockAsync();

        var result = await TryCreateQ3PublisherAsync();
        if (result is null)
        {
            Assert.Inconclusive("Q3 2026 is not open for play. Run LocalDatabaseTool to sync.");
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

        if (biddingCycleBlocked.Count == 0)
        {
            Assert.Inconclusive(
                $"No game has Status == \"{BiddingCycleStatus}\". " +
                "This test only runs when a game was added after the most recent bid cycle. Skipping.");
            return;
        }

        Assert.That(
            biddingCycleBlocked.All(g => g.IsAvailable != true),
            Is.True,
            "Games blocked by the bidding-cycle rule must not be marked available.");
    }

    [Test]
    public async Task PurchaseGame_BiddingCycleBlocked_ReturnsFailure()
    {
        await ResetClockAsync();

        var result = await TryCreateQ3PublisherAsync();
        if (result is null)
        {
            Assert.Inconclusive("Q3 2026 is not open for play. Run LocalDatabaseTool to sync.");
            return;
        }

        using var session = result.Value.Session;
        var publisherID = result.Value.PublisherID;

        var allGames = await session.Royale.PossibleMasterGamesAsync(
            gameName: null, publisherID: publisherID,
            releaseFilter: RoyalePossibleMasterGamesReleaseFilter.All);

        var biddingCycleBlocked = (allGames ?? [])
            .FirstOrDefault(g => g.IsAvailable != true && g.Status == BiddingCycleStatus);

        if (biddingCycleBlocked is null)
        {
            Assert.Inconclusive(
                $"No game has Status == \"{BiddingCycleStatus}\". " +
                "This test only runs when a game was added after the most recent bid cycle. Skipping.");
            return;
        }

        var purchaseResult = await session.Royale.PurchaseGameAsync(
            new PurchaseRoyaleGameRequest
            {
                PublisherID = publisherID,
                MasterGameID = biddingCycleBlocked.MasterGame.MasterGameID,
            });

        Assert.That(purchaseResult!.Success, Is.False,
            "Purchasing a game blocked by the bidding-cycle rule must return Success=false.");
        Assert.That(
            purchaseResult.Errors!.Any(e => e.Contains("bids process", StringComparison.OrdinalIgnoreCase)),
            Is.True,
            "The failure message must reference the bidding-cycle restriction.");
    }

    [Test]
    public async Task SetAdvertisingMoney_AfterPurchase_UpdatesAdvertisingBudget()
    {
        await ResetClockAsync();

        var result = await TryCreateQ3PublisherAsync();
        if (result is null)
        {
            Assert.Inconclusive("Q3 2026 is not open for play. Run LocalDatabaseTool to sync.");
            return;
        }

        using var session = result.Value.Session;
        var publisherID = result.Value.PublisherID;

        var game = await FindPurchasableQ3GameAsync(session, publisherID);
        if (game is null)
        {
            Assert.Inconclusive("No purchasable Q3 2026 games found.");
            return;
        }

        var purchaseResult = await session.Royale.PurchaseGameAsync(
            new PurchaseRoyaleGameRequest
            {
                PublisherID = publisherID,
                MasterGameID = game.MasterGame.MasterGameID,
            });
        Assert.That(purchaseResult!.Success, Is.True,
            $"Setup purchase failed: {string.Join("; ", purchaseResult.Errors ?? [])}");

        await session.Royale.SetAdvertisingMoneyAsync(
            new SetAdvertisingMoneyRequest
            {
                PublisherID = publisherID,
                MasterGameID = game.MasterGame.MasterGameID,
                AdvertisingMoney = 4.0m,
            });

        var publisher = await session.Royale.GetRoyalePublisherAsync(publisherID);
        var purchased = publisher!.PublisherGames!
            .Single(g => g.MasterGame?.MasterGameID == game.MasterGame.MasterGameID);

        Assert.That(purchased.AdvertisingMoney, Is.EqualTo(4.0m),
            "Advertising budget must reflect the assigned amount.");
    }
}
