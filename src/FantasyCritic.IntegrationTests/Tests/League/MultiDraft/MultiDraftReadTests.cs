using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.MultiDraft;

/// <summary>
/// Slice 2 (read path) tests.
///
/// These exercise the read-path primitives that replaced the old <c>DraftNumber = 1</c>
/// filters once a league-year can own more than one draft:
///   - <b>AnyDraftStarted</b> (per-league-year "has any draft started"): surfaced through
///     player-removability (sp_getcombinedleagueyearuserstatus) and the public-leagues
///     listing (GetPublicLeagueYears).
///   - <b>Most-recent-year league type</b> (<c>Standard</c> / <c>OneShot</c> / <c>MultiDraft</c>):
///     surfaced on the home-page league list via sp_getleaguesforuser.
///
/// This deliberately diverges from the original plan, which assumed the league-year exposed a
/// single CurrentDraft PlayStatus. After our changes those signals are booleans/categories, so
/// the assertions target the actual typed surfaces instead.
/// </summary>
[TestFixture]
public class MultiDraftReadTests : IntegrationTestBase
{
    private static async Task<LeagueYearViewModel> AddSecondDraftAsync(LeagueFixture league)
    {
        await league.Manager.LeagueManager.CreateLeagueDraftAsync(new CreateLeagueDraftRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
            Name = "Draft 2",
            ScheduledDate = null,
            GamesToDraft = 2,
            CounterPicksToDraft = 0,
            AdditionalStandardGames = 2,
            AdditionalCounterPicks = 0,
            NewSpecialGameSlots = new List<SpecialGameSlotViewModel>(),
        });

        return await league.GetLeagueYearAsync();
    }

    // ── Primitive A: AnyDraftStarted via player removability ────────────────────────────────

    [Test]
    public async Task Players_AreRemovable_WhenNoDraftHasStarted()
    {
        await using var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(
            Factory, LeagueScenarios.Standard, NewUser);

        var managerUser = await league.Manager.Account.CurrentUserAsync();
        var snapshot = await league.GetLeagueYearAsync();
        var players = snapshot.League.Players!;

        var nonManagerPlayers = players.Where(p => p.UserID != managerUser.UserID).ToList();
        Assert.That(nonManagerPlayers, Is.Not.Empty, "Scenario should have non-manager players.");
        Assert.That(nonManagerPlayers.All(p => p.Removable), Is.True,
            "Before any draft has started, non-manager players should be removable.");
    }

    [Test]
    public async Task Players_AreNotRemovable_AfterADraftHasStarted()
    {
        await using var league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, LeagueScenarios.Standard, NewUser);

        var managerUser = await league.Manager.Account.CurrentUserAsync();
        var snapshot = await league.GetLeagueYearAsync();
        var players = snapshot.League.Players!;

        var nonManagerPlayers = players.Where(p => p.UserID != managerUser.UserID).ToList();
        Assert.That(nonManagerPlayers, Is.Not.Empty, "Scenario should have non-manager players.");
        Assert.That(nonManagerPlayers.All(p => !p.Removable), Is.True,
            "Once a draft has started, players who joined that year should not be removable.");
    }

    // ── Primitive A: AnyDraftStarted on the public-leagues listing ──────────────────────────

    [Test]
    public async Task PublicLeagues_NewLeague_ReportsNoDraftStarted()
    {
        var (email, password, displayName) = NewUser();
        using var manager = new ApiSession(Factory);
        await manager.RegisterAsync(email, password, displayName);

        var year = await LeagueTestHelpers.GetOpenYearAsync(manager);
        var leagueID = await manager.LeagueManager.CreateLeagueAsync(new CreateLeagueRequest
        {
            LeagueName = $"PublicTest-{Guid.NewGuid():N}"[..30],
            PublicLeague = true,
            TestLeague = true,
            CustomRulesLeague = false,
            LeagueYearSettings = LeagueScenarios.Standard.BuildSettings(year),
        });

        var publicLeagues = await manager.League.PublicLeaguesAsync(year, null);
        var thisLeague = publicLeagues.SingleOrDefault(l => l.LeagueID == leagueID);

        Assert.That(thisLeague, Is.Not.Null, "A new public league should appear in the public leagues list.");
        Assert.That(thisLeague!.AnyDraftStarted, Is.False,
            "A league with no started draft should report AnyDraftStarted = false.");
    }

    // ── Primitive B: most-recent-year league type on the home-page list ─────────────────────

    [Test]
    public async Task MyLeagues_StandardSingleDraftLeague_TypeIsStandard()
    {
        await using var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(
            Factory, LeagueScenarios.Standard, NewUser);

        var myLeagues = await league.Manager.League.MyLeaguesAsync(league.Year);
        var thisLeague = myLeagues.Single(l => l.LeagueID == league.LeagueID);

        Assert.That(thisLeague.MostRecentYearType, Is.EqualTo("Standard"));
    }

    [Test]
    public async Task MyLeagues_LeagueWithTwoDrafts_TypeIsMultiDraft()
    {
        await using var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(
            Factory, LeagueScenarios.Standard, NewUser);
        await AddSecondDraftAsync(league);

        var myLeagues = await league.Manager.League.MyLeaguesAsync(league.Year);
        var thisLeague = myLeagues.Single(l => l.LeagueID == league.LeagueID);

        Assert.That(thisLeague.MostRecentYearType, Is.EqualTo("MultiDraft"));
    }

    // ── Read path: the full drafts list is returned for a multi-draft league ────────────────

    [Test]
    public async Task GetLeagueYear_WithTwoDrafts_ReturnsBothDraftsIndependently()
    {
        await using var league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(
            Factory, LeagueScenarios.Standard, NewUser);
        var snapshot = await AddSecondDraftAsync(league);

        Assert.That(snapshot.Drafts.Count, Is.EqualTo(2));
        Assert.That(snapshot.Drafts.Select(d => d.DraftNumber), Is.EquivalentTo(new[] { 1, 2 }));
        Assert.That(snapshot.Drafts.All(d => d.PlayStatus == "NotStartedDraft"), Is.True,
            "Neither draft has started, so both should read as NotStartedDraft.");
    }
}
