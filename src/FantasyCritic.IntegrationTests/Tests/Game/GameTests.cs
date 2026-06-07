using System;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Game;

[TestFixture]
public class GameTests : IntegrationTestBase
{
    // ── Smoke: public read endpoints ──────────────────────────────────────

    [Test]
    public async Task SupportedYears_ReturnsNonEmptyList()
    {
        using var session = new ApiSession(Factory);
        var result = await session.Game.SupportedYearsAsync();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetMasterGameTags_ReturnsNonEmptyList()
    {
        using var session = new ApiSession(Factory);
        var result = await session.Game.GetMasterGameTagsAsync();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task MasterGameAll_ReturnsNonEmptyList()
    {
        using var session = new ApiSession(Factory);
        var result = await session.Game.MasterGameAllAsync();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    // ── Not-found cases ───────────────────────────────────────────────────

    [Test]
    public async Task MasterGame_UnknownID_Returns404()
    {
        using var session = new ApiSession(Factory);
        ApiException? ex = null;
        try
        {
            await session.Game.MasterGameAsync(Guid.NewGuid());
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for unknown game ID.");
        Assert.That(ex!.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task MasterGameChangeLog_UnknownID_Returns404()
    {
        using var session = new ApiSession(Factory);
        ApiException? ex = null;
        try
        {
            await session.Game.MasterGameChangeLogAsync(Guid.NewGuid());
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for unknown game ID.");
        Assert.That(ex!.StatusCode, Is.EqualTo(404));
    }

    // ── Auth edge cases ───────────────────────────────────────────────────

    [Test]
    public async Task MyMasterGameRequests_Unauthenticated_Returns401()
    {
        using var session = new ApiSession(Factory);
        ApiException? ex = null;
        try
        {
            await session.Game.MyMasterGameRequestsAsync();
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for unauthenticated request.");
        Assert.That(ex!.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task MyMasterGameChangeRequests_Unauthenticated_Returns401()
    {
        using var session = new ApiSession(Factory);
        ApiException? ex = null;
        try
        {
            await session.Game.MyMasterGameChangeRequestsAsync();
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for unauthenticated request.");
        Assert.That(ex!.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task LeagueYearsWithMasterGame_Unauthenticated_ReturnsEmptyList()
    {
        using var session = new ApiSession(Factory);
        var result = await session.Game.LeagueYearsWithMasterGameAsync(Guid.NewGuid());
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
    }
}
