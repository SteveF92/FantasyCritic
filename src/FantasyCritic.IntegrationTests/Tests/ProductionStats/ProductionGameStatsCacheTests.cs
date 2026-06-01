using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.IntegrationTests.ProductionStats;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.ProductionStats;

[TestFixture]
public class ProductionGameStatsCacheTests
{
    private sealed record Candidate(Guid MasterGameID, string Label);

    [Test]
    public void SelectHighestHypeAvailable_WhenCandidatesEmpty_ReturnsNull()
    {
        var stats = new List<ProductionMasterGameYearStat>
        {
            new(Guid.NewGuid(), "Game A", 10m)
        };

        var result = ProductionGameStatsCache.SelectHighestHypeAvailable(
            Array.Empty<Candidate>(),
            c => c.MasterGameID,
            stats);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void SelectHighestHypeAvailable_WhenStatsEmpty_ReturnsFirstCandidate()
    {
        var id1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var id2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var candidates = new[]
        {
            new Candidate(id1, "first"),
            new Candidate(id2, "second")
        };

        var result = ProductionGameStatsCache.SelectHighestHypeAvailable(
            candidates,
            c => c.MasterGameID,
            Array.Empty<ProductionMasterGameYearStat>());

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.MasterGameID, Is.EqualTo(id1));
    }

    [Test]
    public void SelectHighestHypeAvailable_WhenStatsPresent_ReturnsHighestHypeCandidate()
    {
        var lowId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var highId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var missingId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        var candidates = new[]
        {
            new Candidate(lowId, "low"),
            new Candidate(highId, "high"),
            new Candidate(missingId, "missing-from-stats")
        };

        var stats = new List<ProductionMasterGameYearStat>
        {
            new(lowId, "Low Hype Game", 1.5m),
            new(highId, "High Hype Game", 99.9m)
        };

        var result = ProductionGameStatsCache.SelectHighestHypeAvailable(
            candidates,
            c => c.MasterGameID,
            stats);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.MasterGameID, Is.EqualTo(highId));
    }

    private sealed record CostCandidate(Guid Id, decimal Cost);

    [Test]
    public void SelectAffordableSet_WhenCandidatesEmpty_ReturnsEmpty()
    {
        var result = ProductionGameStatsCache.SelectAffordableSet(
            Array.Empty<CostCandidate>(),
            c => c.Id,
            c => c.Cost,
            maxCount: 10,
            budgetCap: 100m,
            Array.Empty<ProductionMasterGameYearStat>());

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void SelectAffordableSet_WhenBudgetCapExcludesCostlyGame_ExcludesThatGame()
    {
        var cheapID    = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var expensiveID = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var candidates = new[]
        {
            new CostCandidate(cheapID, 5m),
            new CostCandidate(expensiveID, 50m),
        };

        var result = ProductionGameStatsCache.SelectAffordableSet(
            candidates,
            c => c.Id,
            c => c.Cost,
            maxCount: 10,
            budgetCap: 20m,
            Array.Empty<ProductionMasterGameYearStat>());

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(cheapID));
    }

    [Test]
    public void SelectAffordableSet_WhenMaxCountReached_StopsAtCount()
    {
        var candidates = Enumerable.Range(0, 5)
            .Select(_ => new CostCandidate(Guid.NewGuid(), 1m))
            .ToList();

        var result = ProductionGameStatsCache.SelectAffordableSet(
            candidates,
            c => c.Id,
            c => c.Cost,
            maxCount: 3,
            budgetCap: 100m,
            Array.Empty<ProductionMasterGameYearStat>());

        Assert.That(result.Count, Is.EqualTo(3));
    }

    [Test]
    public void SelectAffordableSet_WhenStatsPresent_PicksHighestHypeFirst()
    {
        var lowHypeID  = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var highHypeID = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var candidates = new[]
        {
            new CostCandidate(lowHypeID, 10m),
            new CostCandidate(highHypeID, 10m),
        };
        var stats = new[]
        {
            new ProductionMasterGameYearStat(lowHypeID,  "Low Hype Game",  1m),
            new ProductionMasterGameYearStat(highHypeID, "High Hype Game", 99m),
        };

        var result = ProductionGameStatsCache.SelectAffordableSet(
            candidates,
            c => c.Id,
            c => c.Cost,
            maxCount: 1,
            budgetCap: 15m,
            stats);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(highHypeID),
            "Highest-hype game should be selected first when budget only allows one.");
    }
}
