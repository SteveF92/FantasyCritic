using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.ApiModels;
using FantasyCritic.IntegrationTests.Helpers;
using FantasyCritic.IntegrationTests.ProductionStats;
using FantasyCritic.Web.Models.Requests.Royale;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Royale;

[TestFixture]
public class RoyaleTests : IntegrationTestBase
{
    [Test]
    public async Task CreateRoyalePublisher_InActiveQuarter_Succeeds()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var activeQuarter = await session.GetAndDeserializeAsync<RoyaleYearQuarterJson>(
            "/api/Royale/ActiveRoyaleQuarter");
        Assert.That(activeQuarter.OpenForPlay, Is.True, "Active quarter must be open for play.");
        Assert.That(activeQuarter.Finished, Is.False, "Active quarter must not be finished.");

        var publisherName = $"Pub-{Guid.NewGuid():N}"[..20];
        var publisherID = await session.PostJsonAndDeserializeAsync<CreateRoyalePublisherRequest, Guid>(
            "/api/Royale/CreateRoyalePublisher",
            new CreateRoyalePublisherRequest(activeQuarter.Year, activeQuarter.Quarter, publisherName));

        Assert.That(publisherID, Is.Not.EqualTo(Guid.Empty));

        var publisher = await session.GetAndDeserializeAsync<RoyalePublisherJson>(
            $"/api/Royale/GetRoyalePublisher/{publisherID}");
        Assert.That(publisher.PublisherName, Is.EqualTo(publisherName));
        Assert.That(publisher.YearQuarter.Year, Is.EqualTo(activeQuarter.Year));
        Assert.That(publisher.YearQuarter.Quarter, Is.EqualTo(activeQuarter.Quarter));
    }

    [Test]
    public async Task PurchaseGame_WhenGameIsAvailable_Succeeds()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var activeQuarter = await session.GetAndDeserializeAsync<RoyaleYearQuarterJson>(
            "/api/Royale/ActiveRoyaleQuarter");

        var publisherName = $"Pub-{Guid.NewGuid():N}"[..20];
        var publisherID = await session.PostJsonAndDeserializeAsync<CreateRoyalePublisherRequest, Guid>(
            "/api/Royale/CreateRoyalePublisher",
            new CreateRoyalePublisherRequest(activeQuarter.Year, activeQuarter.Quarter, publisherName));

        var gameToBuy = await FindPurchasableGameViaApiAsync(session, publisherID, activeQuarter.Year);
        Assert.That(
            gameToBuy,
            Is.Not.Null,
            "PossibleMasterGames did not return any purchasable game for the active quarter "
            + "(tried each release filter, same as the purchase-game UI).");

        var purchaseResult = await session.PostJsonAndDeserializeAsync<PurchaseRoyaleGameRequest, PlayerClaimResultJson>(
            "/api/Royale/PurchaseGame",
            new PurchaseRoyaleGameRequest(publisherID, gameToBuy!.MasterGame.MasterGameID));

        Assert.That(purchaseResult.Success, Is.True, string.Join("; ", purchaseResult.Errors));

        var publisher = await session.GetAndDeserializeAsync<RoyalePublisherJson>(
            $"/api/Royale/GetRoyalePublisher/{publisherID}");
        var purchasedMasterGameID = gameToBuy.MasterGame.MasterGameID;
        Assert.That(
            publisher.PublisherGames.Any(g => g.MasterGame?.MasterGameID == purchasedMasterGameID),
            Is.True,
            "Purchased game should appear on the publisher roster.");
    }

    [Test]
    public async Task ChangePublisherProfile_NameIconAndSlogan_Succeeds()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var activeQuarter = await session.GetAndDeserializeAsync<RoyaleYearQuarterJson>(
            "/api/Royale/ActiveRoyaleQuarter");

        var initialName = $"Pub-{Guid.NewGuid():N}"[..20];
        var publisherID = await session.PostJsonAndDeserializeAsync<CreateRoyalePublisherRequest, Guid>(
            "/api/Royale/CreateRoyalePublisher",
            new CreateRoyalePublisherRequest(activeQuarter.Year, activeQuarter.Quarter, initialName));

        var newName = $"Pub-{Guid.NewGuid():N}"[..20];

        var nameResponse = await session.PostJsonAsync("/api/Royale/ChangePublisherName",
            new ChangeRoyalePublisherNameRequest(publisherID, newName));
        Assert.That(nameResponse.IsSuccessStatusCode, Is.True, "ChangePublisherName should return 2xx.");

        var iconResponse = await session.PostJsonAsync("/api/Royale/ChangePublisherIcon",
            new ChangeRoyalePublisherIconRequest(publisherID, "🎮"));
        Assert.That(iconResponse.IsSuccessStatusCode, Is.True, "ChangePublisherIcon should return 2xx.");

        var sloganResponse = await session.PostJsonAsync("/api/Royale/ChangePublisherSlogan",
            new ChangeRoyalePublisherSloganRequest(publisherID, "Test Slogan"));
        Assert.That(sloganResponse.IsSuccessStatusCode, Is.True, "ChangePublisherSlogan should return 2xx.");

        var publisher = await session.GetAndDeserializeAsync<RoyalePublisherJson>(
            $"/api/Royale/GetRoyalePublisher/{publisherID}");

        Assert.That(publisher.PublisherName, Is.EqualTo(newName));
        Assert.That(publisher.PublisherIcon, Is.EqualTo("🎮"));
        Assert.That(publisher.PublisherSlogan, Is.EqualTo("Test Slogan"));
    }

    [Test]
    public async Task RoyaleLifecycle_MultiPurchaseAdvertiseSellRebuy_Succeeds()
    {
        // ── Setup ──────────────────────────────────────────────────────────────
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var activeQuarter = await session.GetAndDeserializeAsync<RoyaleYearQuarterJson>(
            "/api/Royale/ActiveRoyaleQuarter");

        var publisherName = $"Pub-{Guid.NewGuid():N}"[..20];
        var publisherID = await session.PostJsonAndDeserializeAsync<CreateRoyalePublisherRequest, Guid>(
            "/api/Royale/CreateRoyalePublisher",
            new CreateRoyalePublisherRequest(activeQuarter.Year, activeQuarter.Quarter, publisherName));

        // ── Step A: Build up to a 10-game roster ───────────────────────────────
        // Keep $9 in reserve ($1 + $3 + $5 advertising allocated below).
        const decimal adReserve = 9m;
        const decimal budgetCap = 100m - adReserve;

        var gameSet = await FindAffordableSetViaApiAsync(
            session, publisherID, activeQuarter.Year, maxCount: 10, budgetCap);

        Assert.That(gameSet.Count, Is.GreaterThan(0),
            "Expected at least one purchasable game for the active quarter. "
            + "Verify the local DB is synced from production (run LocalDatabaseTool).");

        var purchasedIDs = new List<Guid>();
        foreach (var game in gameSet)
        {
            var result = await session.PostJsonAndDeserializeAsync<PurchaseRoyaleGameRequest, PlayerClaimResultJson>(
                "/api/Royale/PurchaseGame",
                new PurchaseRoyaleGameRequest(publisherID, game.MasterGame.MasterGameID));
            Assert.That(result.Success, Is.True,
                $"PurchaseGame failed for {game.MasterGame.MasterGameID}: {string.Join("; ", result.Errors)}");
            purchasedIDs.Add(game.MasterGame.MasterGameID);
        }

        var publisher = await session.GetAndDeserializeAsync<RoyalePublisherJson>(
            $"/api/Royale/GetRoyalePublisher/{publisherID}");

        Assert.That(publisher.PublisherGames.Count, Is.EqualTo(gameSet.Count),
            "Roster count should match the number of successful purchases.");
        Assert.That(publisher.Budget, Is.GreaterThanOrEqualTo(adReserve),
            "Budget should have at least $9 remaining for advertising.");

        // ── Step B: Advertising budgets ────────────────────────────────────────
        Assert.That(purchasedIDs.Count, Is.GreaterThanOrEqualTo(3),
            "Need at least 3 games to set advertising money on games 0, 1, 2.");

        var game0ID = purchasedIDs[0];
        var game1ID = purchasedIDs[1];
        var game2ID = purchasedIDs[2];

        var ad0 = await session.PostJsonAsync("/api/Royale/SetAdvertisingMoney",
            new SetAdvertisingMoneyRequest(publisherID, game0ID, 1m));
        Assert.That(ad0.IsSuccessStatusCode, Is.True, "SetAdvertisingMoney $1 on game0 should succeed.");

        var ad1 = await session.PostJsonAsync("/api/Royale/SetAdvertisingMoney",
            new SetAdvertisingMoneyRequest(publisherID, game1ID, 3m));
        Assert.That(ad1.IsSuccessStatusCode, Is.True, "SetAdvertisingMoney $3 on game1 should succeed.");

        var ad2 = await session.PostJsonAsync("/api/Royale/SetAdvertisingMoney",
            new SetAdvertisingMoneyRequest(publisherID, game2ID, 5m));
        Assert.That(ad2.IsSuccessStatusCode, Is.True, "SetAdvertisingMoney $5 on game2 should succeed.");

        // Owner reloads — hidden games still have MasterGame populated for the owner.
        publisher = await session.GetAndDeserializeAsync<RoyalePublisherJson>(
            $"/api/Royale/GetRoyalePublisher/{publisherID}");

        Assert.That(
            publisher.PublisherGames.Single(g => g.MasterGame!.MasterGameID == game0ID).AdvertisingMoney,
            Is.EqualTo(1m), "game0 should have $1 advertising money.");
        Assert.That(
            publisher.PublisherGames.Single(g => g.MasterGame!.MasterGameID == game1ID).AdvertisingMoney,
            Is.EqualTo(3m), "game1 should have $3 advertising money.");
        Assert.That(
            publisher.PublisherGames.Single(g => g.MasterGame!.MasterGameID == game2ID).AdvertisingMoney,
            Is.EqualTo(5m), "game2 should have $5 advertising money.");

        // ── Step C: Sell game0 and buy a replacement ───────────────────────────
        var budgetBeforeSell = publisher.Budget;

        var sellResponse = await session.PostJsonAsync("/api/Royale/SellGame",
            new SellRoyaleGameRequest(publisherID, game0ID));
        Assert.That(sellResponse.IsSuccessStatusCode, Is.True, "SellGame should return 2xx.");

        publisher = await session.GetAndDeserializeAsync<RoyalePublisherJson>(
            $"/api/Royale/GetRoyalePublisher/{publisherID}");

        Assert.That(
            publisher.PublisherGames.All(g => g.MasterGame?.MasterGameID != game0ID),
            Is.True,
            "Sold game (game0) should not appear on the roster after selling.");
        Assert.That(publisher.Budget, Is.GreaterThan(budgetBeforeSell),
            "Budget should increase after selling (refund applied).");

        var replacement = await FindPurchasableGameViaApiAsync(session, publisherID, activeQuarter.Year);
        Assert.That(replacement, Is.Not.Null,
            "A purchasable replacement game should exist after selling game0.");

        var replaceResult = await session.PostJsonAndDeserializeAsync<PurchaseRoyaleGameRequest, PlayerClaimResultJson>(
            "/api/Royale/PurchaseGame",
            new PurchaseRoyaleGameRequest(publisherID, replacement!.MasterGame.MasterGameID));
        Assert.That(replaceResult.Success, Is.True,
            $"Replacement purchase failed: {string.Join("; ", replaceResult.Errors)}");

        publisher = await session.GetAndDeserializeAsync<RoyalePublisherJson>(
            $"/api/Royale/GetRoyalePublisher/{publisherID}");
        Assert.That(
            publisher.PublisherGames.Any(g => g.MasterGame?.MasterGameID == replacement.MasterGame.MasterGameID),
            Is.True,
            "Replacement game should appear on the roster.");

        // ── Step D: Public visibility check ────────────────────────────────────
        // A different authenticated user viewing this publisher sees hidden-game entries
        // with GameHidden=true but MasterGame=null (game identity concealed).
        // The owner, by contrast, sees MasterGame populated even for hidden games.
        var (email2, password2, displayName2) = NewUser();
        using var publicSession = new ApiSession(Factory);
        await publicSession.RegisterAsync(email2, password2, displayName2);

        var publicView = await publicSession.GetAndDeserializeAsync<RoyalePublisherJson>(
            $"/api/Royale/GetRoyalePublisher/{publisherID}");

        Assert.That(publicView.PublisherGames.Count, Is.GreaterThan(0),
            "Public viewer should be able to see how many games the publisher has.");

        var hiddenFromPublic = publicView.PublisherGames.Where(g => g.GameHidden).ToList();
        Assert.That(hiddenFromPublic.Count, Is.GreaterThan(0),
            "At least one game should be hidden from a public viewer. "
            + "All purchased games are unreleased and unscored, so all should be hidden.");

        foreach (var hidden in hiddenFromPublic)
        {
            Assert.That(hidden.MasterGame, Is.Null,
                "Public viewers must not see the MasterGame identity of hidden games.");
            Assert.That(hidden.AmountSpent, Is.Null,
                "Public viewers must not see the AmountSpent of hidden games.");
        }

        // Owner still sees MasterGame for their own hidden games.
        var ownerView = await session.GetAndDeserializeAsync<RoyalePublisherJson>(
            $"/api/Royale/GetRoyalePublisher/{publisherID}");

        foreach (var ownerHidden in ownerView.PublisherGames.Where(g => g.GameHidden))
        {
            Assert.That(ownerHidden.MasterGame, Is.Not.Null,
                "Owner must see MasterGame even for games flagged as hidden.");
        }
    }

    [Test]
    public async Task SetAdvertisingMoney_OverTenDollars_ReturnsBadRequest()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var activeQuarter = await session.GetAndDeserializeAsync<RoyaleYearQuarterJson>(
            "/api/Royale/ActiveRoyaleQuarter");

        var publisherName = $"Pub-{Guid.NewGuid():N}"[..20];
        var publisherID = await session.PostJsonAndDeserializeAsync<CreateRoyalePublisherRequest, Guid>(
            "/api/Royale/CreateRoyalePublisher",
            new CreateRoyalePublisherRequest(activeQuarter.Year, activeQuarter.Quarter, publisherName));

        var game = await FindPurchasableGameViaApiAsync(session, publisherID, activeQuarter.Year);
        Assert.That(game, Is.Not.Null,
            "Need a purchasable game to set advertising money on. "
            + "Verify the local DB is synced from production.");

        var purchaseResult = await session.PostJsonAndDeserializeAsync<PurchaseRoyaleGameRequest, PlayerClaimResultJson>(
            "/api/Royale/PurchaseGame",
            new PurchaseRoyaleGameRequest(publisherID, game!.MasterGame.MasterGameID));
        Assert.That(purchaseResult.Success, Is.True,
            $"Setup purchase failed: {string.Join("; ", purchaseResult.Errors)}");

        // Attempt to set $10.01 — one cent above the $10 cap
        var overCapResponse = await session.PostJsonAsync("/api/Royale/SetAdvertisingMoney",
            new SetAdvertisingMoneyRequest(publisherID, game.MasterGame.MasterGameID, 10.01m));

        Assert.That(overCapResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
            "Setting advertising money above $10 must return HTTP 400 Bad Request.");
    }

    [Test]
    public async Task PurchaseGame_WhenBudgetInsufficient_ReturnsFailure()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var activeQuarter = await session.GetAndDeserializeAsync<RoyaleYearQuarterJson>(
            "/api/Royale/ActiveRoyaleQuarter");

        var publisherName = $"Pub-{Guid.NewGuid():N}"[..20];
        var publisherID = await session.PostJsonAndDeserializeAsync<CreateRoyalePublisherRequest, Guid>(
            "/api/Royale/CreateRoyalePublisher",
            new CreateRoyalePublisherRequest(activeQuarter.Year, activeQuarter.Quarter, publisherName));

        // Buy the most expensive available game repeatedly to deplete budget quickly.
        var boughtIDs = new List<Guid>();
        while (true)
        {
            var all = await session.GetAndDeserializeAsync<List<PossibleRoyaleMasterGameJson>>(
                $"/api/Royale/PossibleMasterGames?publisherID={publisherID}&releaseFilter=All");

            var next = all.Where(g => g.IsAvailable).MaxBy(g => g.Cost);
            if (next is null) break; // No more affordable games

            var result = await session.PostJsonAndDeserializeAsync<PurchaseRoyaleGameRequest, PlayerClaimResultJson>(
                "/api/Royale/PurchaseGame",
                new PurchaseRoyaleGameRequest(publisherID, next.MasterGame.MasterGameID));
            if (!result.Success) break;
            boughtIDs.Add(next.MasterGame.MasterGameID);
        }

        if (boughtIDs.Count == 0)
        {
            Assert.Inconclusive("No games were purchasable; cannot verify purchase failure path.");
            return;
        }

        // Best target: a game blocked by "Not enough budget." if one exists;
        // otherwise fall back to re-purchasing an already-owned game.
        var allGames = await session.GetAndDeserializeAsync<List<PossibleRoyaleMasterGameJson>>(
            $"/api/Royale/PossibleMasterGames?publisherID={publisherID}&releaseFilter=All");

        var budgetBlocked = allGames.FirstOrDefault(g => !g.IsAvailable && g.Status == "Not enough budget.");
        var targetID = budgetBlocked?.MasterGame.MasterGameID ?? boughtIDs[0];

        var failureResult = await session.PostJsonAndDeserializeAsync<PurchaseRoyaleGameRequest, PlayerClaimResultJson>(
            "/api/Royale/PurchaseGame",
            new PurchaseRoyaleGameRequest(publisherID, targetID));

        Assert.That(failureResult.Success, Is.False,
            "Purchasing a game the publisher cannot afford (or already owns) must return Success=false.");
        Assert.That(failureResult.Errors, Is.Not.Empty,
            "A failed purchase must include at least one error message.");
    }

    [Test]
    public async Task PurchaseGame_InLockoutWindow_ReturnsFailure()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var activeQuarter = await session.GetAndDeserializeAsync<RoyaleYearQuarterJson>(
            "/api/Royale/ActiveRoyaleQuarter");

        var publisherName = $"Pub-{Guid.NewGuid():N}"[..20];
        var publisherID = await session.PostJsonAndDeserializeAsync<CreateRoyalePublisherRequest, Guid>(
            "/api/Royale/CreateRoyalePublisher",
            new CreateRoyalePublisherRequest(activeQuarter.Year, activeQuarter.Quarter, publisherName));

        var allGames = await session.GetAndDeserializeAsync<List<PossibleRoyaleMasterGameJson>>(
            $"/api/Royale/PossibleMasterGames?publisherID={publisherID}&releaseFilter=All");

        // The service sets Status = "Game will release within 5 days." for games in the lockout window.
        // RoyaleService.FUTURE_RELEASE_LIMIT_DAYS == 5.
        const string lockoutStatus = "Game will release within 5 days.";
        var lockoutGame = allGames.FirstOrDefault(g => !g.IsAvailable && g.Status == lockoutStatus);

        if (lockoutGame is null)
        {
            Assert.Inconclusive(
                $"No game currently has Status == \"{lockoutStatus}\". "
                + "This test only runs when a game is releasing within 5 days. Skipping.");
            return;
        }

        var result = await session.PostJsonAndDeserializeAsync<PurchaseRoyaleGameRequest, PlayerClaimResultJson>(
            "/api/Royale/PurchaseGame",
            new PurchaseRoyaleGameRequest(publisherID, lockoutGame.MasterGame.MasterGameID));

        Assert.That(result.Success, Is.False,
            "Purchasing a game in the 5-day lockout window must return Success=false.");
        Assert.That(result.Errors, Is.Not.Empty,
            "A failed purchase must include at least one error message.");
    }

    /// <summary>
    /// Mirrors the purchase-game modal: list top games under each release filter until one is available.
    /// Picks the highest-hype candidate using production stats when reachable.
    /// </summary>
    private static async Task<PossibleRoyaleMasterGameJson?> FindPurchasableGameViaApiAsync(
        ApiSession session,
        Guid publisherID,
        int year)
    {
        foreach (var releaseFilter in new[] { "All", "ExpectedToReleaseInQuarter", "ConfirmedReleaseInQuarter" })
        {
            var possibleGames = await session.GetAndDeserializeAsync<List<PossibleRoyaleMasterGameJson>>(
                $"/api/Royale/PossibleMasterGames?publisherID={publisherID}&releaseFilter={releaseFilter}");

            var available = possibleGames.Where(g => g.IsAvailable).ToList();
            if (available.Count > 0)
            {
                return await ProductionGameStatsCache.FindHighestHypeAvailableAsync(
                    available,
                    g => g.MasterGame.MasterGameID,
                    year);
            }
        }

        return null;
    }

    /// <summary>
    /// Tries ExpectedToReleaseInQuarter first; falls back to All if no candidates found.
    /// Returns up to <paramref name="maxCount"/> games whose combined cost stays within
    /// <paramref name="budgetCap"/>, ordered by production HypeFactor descending.
    /// </summary>
    private static async Task<IReadOnlyList<PossibleRoyaleMasterGameJson>> FindAffordableSetViaApiAsync(
        ApiSession session,
        Guid publisherID,
        int year,
        int maxCount,
        decimal budgetCap)
    {
        foreach (var releaseFilter in new[] { "ExpectedToReleaseInQuarter", "All" })
        {
            var possibleGames = await session.GetAndDeserializeAsync<List<PossibleRoyaleMasterGameJson>>(
                $"/api/Royale/PossibleMasterGames?publisherID={publisherID}&releaseFilter={releaseFilter}");

            var available = possibleGames.Where(g => g.IsAvailable).ToList();
            if (available.Count == 0) continue;

            var set = await ProductionGameStatsCache.FindAffordableSetAsync(
                available,
                g => g.MasterGame.MasterGameID,
                g => g.Cost,
                maxCount,
                budgetCap,
                year);

            if (set.Count > 0) return set;
        }

        return Array.Empty<PossibleRoyaleMasterGameJson>();
    }
}
