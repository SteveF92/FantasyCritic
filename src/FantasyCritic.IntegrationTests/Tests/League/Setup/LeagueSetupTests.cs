using System;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Setup;

[TestFixture]
public class LeagueSetupTests : IntegrationTestBase
{
    [Test]
    public async Task LeagueOptions_ReturnsAtLeastOneOpenYear()
    {
        using var session = new ApiSession(Factory);
        var options = await session.League.LeagueOptionsAsync();

        Assert.That(options, Is.Not.Null);
        Assert.That(options.OpenYears, Is.Not.Empty, "At least one year must be open for creation.");
    }

    [Test]
    public async Task LeagueOptions_DraftSystems_ContainsFlexible()
    {
        using var session = new ApiSession(Factory);
        var options = await session.League.LeagueOptionsAsync();

        Assert.That(options.DraftSystems, Contains.Item("Flexible"),
            "DraftSystems must include 'Flexible'.");
    }

    [Test]
    public async Task CreateLeague_WithValidSettings_ReturnsNonEmptyGuid()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(session);
        var leagueID = await LeagueTestHelpers.CreateLeagueAsync(session, LeagueScenarios.Standard, year);

        Assert.That(leagueID, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task GetLeague_AfterCreate_ReturnsLeagueWithCorrectManagerFlag()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(session);
        var leagueID = await LeagueTestHelpers.CreateLeagueAsync(session, LeagueScenarios.Standard, year);

        var league = await session.League.GetLeagueAsync(leagueID, null);

        Assert.That(league, Is.Not.Null);
        Assert.That(league.LeagueID, Is.EqualTo(leagueID));
        Assert.That(league.IsManager, Is.True, "The creator must be the league manager.");
        Assert.That(league.TestLeague, Is.True);
    }

    [Test]
    public async Task GetLeagueYearOptions_AfterCreate_RoundTripsStandardGameCount()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(session);
        var leagueID = await LeagueTestHelpers.CreateLeagueAsync(session, LeagueScenarios.Standard, year);

        var settings = await session.League.GetLeagueYearOptionsAsync(leagueID, year);

        Assert.That(settings, Is.Not.Null);
        Assert.That(settings.StandardGames, Is.EqualTo(LeagueScenarios.Standard.StandardGames));
        Assert.That(settings.GamesToDraft, Is.EqualTo(LeagueScenarios.Standard.GamesToDraft));
        Assert.That(settings.CounterPicks, Is.EqualTo(LeagueScenarios.Standard.CounterPicks));
        Assert.That(settings.DraftSystem, Is.EqualTo(LeagueScenarios.Standard.DraftSystem));
        Assert.That(settings.ScoringSystem, Is.EqualTo(LeagueScenarios.Standard.ScoringSystem));
        Assert.That(settings.IneligibleGameSystem, Is.EqualTo(LeagueScenarios.Standard.IneligibleGameSystem));
    }

    [Test]
    public async Task GetLeagueYear_WithGrantSuperDrops_BeforeDraft_ReturnsNullSuperDropPointCutoff()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(session);
        var leagueID = await LeagueTestHelpers.CreateLeagueAsync(
            session, LeagueScenarios.StandardWithSuperDrops, year);

        var leagueYear = await session.League.GetLeagueYearAsync(leagueID, year, null);

        Assert.That(leagueYear.Settings.GrantSuperDrops, Is.True);
        Assert.That(leagueYear.SuperDropPointCutoff, Is.Null,
            "Super drop cutoff must not be computed until the draft has finished.");
    }
}
