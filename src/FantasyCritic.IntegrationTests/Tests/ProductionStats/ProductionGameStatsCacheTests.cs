using System;
using System.Collections.Generic;
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
}
