using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Setup;

[TestFixture]
public class MultiDraftCreationTests : IntegrationTestBase
{
    [Test]
    public async Task CreateLeague_WithTwoDrafts_CreatesBothDraftRows()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(session);
        var scenario = LeagueScenarios.Standard;

        var leagueID = await session.LeagueManager.CreateLeagueAsync(new CreateLeagueRequest
        {
            LeagueName = $"TestLeague-{Guid.NewGuid():N}"[..30],
            PublicLeague = false,
            TestLeague = true,
            CustomRulesLeague = false,
            LeagueYearSettings = scenario.BuildSettings(year),
            Drafts = new List<DraftSettingsRequest>
            {
                new() { Name = null, ScheduledDate = null, GamesToDraft = 3, CounterPicksToDraft = 1 },
                new() { Name = "Draft 2", ScheduledDate = null, GamesToDraft = 3, CounterPicksToDraft = 0 },
            }
        });

        var leagueYear = await session.League.GetLeagueYearAsync(leagueID, year, null);

        Assert.That(leagueYear, Is.Not.Null);
        Assert.That(leagueYear.Drafts, Has.Count.EqualTo(2));

        var first = leagueYear.Drafts.Single(d => d.DraftNumber == 1);
        Assert.That(first.Name, Is.EqualTo("Initial Draft"));
        Assert.That(first.GamesToDraft, Is.EqualTo(3));
        Assert.That(first.CounterPicksToDraft, Is.EqualTo(1));

        var second = leagueYear.Drafts.Single(d => d.DraftNumber == 2);
        Assert.That(second.Name, Is.EqualTo("Draft 2"));
        Assert.That(second.GamesToDraft, Is.EqualTo(3));
        Assert.That(second.CounterPicksToDraft, Is.EqualTo(0));
    }

    [Test]
    public async Task CreateLeague_WithThreeDrafts_CreatesThreeDraftRows()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(session);
        var scenario = LeagueScenarios.Standard;

        // Standard scenario has StandardGames = 6; use 2+2+2 split
        var leagueID = await session.LeagueManager.CreateLeagueAsync(new CreateLeagueRequest
        {
            LeagueName = $"TestLeague-{Guid.NewGuid():N}"[..30],
            PublicLeague = false,
            TestLeague = true,
            CustomRulesLeague = false,
            LeagueYearSettings = scenario.BuildSettings(year),
            Drafts = new List<DraftSettingsRequest>
            {
                new() { Name = null, ScheduledDate = null, GamesToDraft = 2, CounterPicksToDraft = 1 },
                new() { Name = null, ScheduledDate = null, GamesToDraft = 2, CounterPicksToDraft = 0 },
                new() { Name = null, ScheduledDate = null, GamesToDraft = 2, CounterPicksToDraft = 0 },
            }
        });

        var leagueYear = await session.League.GetLeagueYearAsync(leagueID, year, null);

        Assert.That(leagueYear.Drafts, Has.Count.EqualTo(3));
        Assert.That(leagueYear.Drafts.Select(d => d.DraftNumber).Order(), Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public async Task EditLeagueYearSettings_WithFirstDraft_UpdatesDraftCounts()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(session);
        var leagueID = await LeagueTestHelpers.CreateLeagueAsync(session, LeagueScenarios.Standard, year);

        var settings = await session.League.GetLeagueYearOptionsAsync(leagueID, year);

        await session.LeagueManager.EditLeagueYearSettingsAsync(new EditLeagueYearRequest
        {
            LeagueID = leagueID,
            Year = year,
            LeagueYearName = null,
            LeagueYearSettings = settings,
            FirstDraft = new() { Name = null, ScheduledDate = null, GamesToDraft = 4, CounterPicksToDraft = 1 },
        });

        var leagueYear = await session.League.GetLeagueYearAsync(leagueID, year, null);
        var firstDraft = leagueYear.Drafts.Single(d => d.DraftNumber == 1);
        Assert.That(firstDraft.GamesToDraft, Is.EqualTo(4));
        Assert.That(firstDraft.CounterPicksToDraft, Is.EqualTo(1));
    }

    [Test]
    public async Task EditLeagueYearSettings_MultiDraftLeague_FirstDraftIgnored()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(session);

        // Create with 2 drafts
        var leagueID = await session.LeagueManager.CreateLeagueAsync(new CreateLeagueRequest
        {
            LeagueName = $"TestLeague-{Guid.NewGuid():N}"[..30],
            PublicLeague = false,
            TestLeague = true,
            CustomRulesLeague = false,
            LeagueYearSettings = LeagueScenarios.Standard.BuildSettings(year),
            Drafts = new List<DraftSettingsRequest>
            {
                new() { Name = null, ScheduledDate = null, GamesToDraft = 3, CounterPicksToDraft = 1 },
                new() { Name = null, ScheduledDate = null, GamesToDraft = 3, CounterPicksToDraft = 0 },
            }
        });

        var originalLeagueYear = await session.League.GetLeagueYearAsync(leagueID, year, null);
        var originalGamesToDraft = originalLeagueYear.Drafts.Single(d => d.DraftNumber == 1).GamesToDraft;

        var settings = await session.League.GetLeagueYearOptionsAsync(leagueID, year);

        // Send FirstDraft with different counts — should be silently ignored for multi-draft leagues
        await session.LeagueManager.EditLeagueYearSettingsAsync(new EditLeagueYearRequest
        {
            LeagueID = leagueID,
            Year = year,
            LeagueYearName = null,
            LeagueYearSettings = settings,
            FirstDraft = new() { Name = null, ScheduledDate = null, GamesToDraft = 99, CounterPicksToDraft = 99 },
        });

        var updatedLeagueYear = await session.League.GetLeagueYearAsync(leagueID, year, null);
        Assert.That(updatedLeagueYear.Drafts.Single(d => d.DraftNumber == 1).GamesToDraft, Is.EqualTo(originalGamesToDraft));
    }
}
