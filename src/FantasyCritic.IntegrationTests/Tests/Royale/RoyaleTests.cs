using System;
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
}
