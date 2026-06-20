using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.MultiDraft;

/// <summary>
/// Tests for creating, editing, and deleting additional league drafts (Slice 1).
///
/// OneTimeSetUp creates a fully initialised league (publishers created, draft order set, draft
/// not started) via <see cref="LeagueFixtureBuilder.CreateLeagueWithMembersAsync"/>.
/// Each test is self-contained: it creates any draft it needs as part of the test body so
/// that tests do not interfere with one another's draft counts.
/// </summary>
[TestFixture]
public class MultiDraftCrudTests : IntegrationTestBase
{
    private LeagueFixture _league = null!;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _league = await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(
            Factory, LeagueScenarios.Standard, NewUser);
    }

    [OneTimeTearDown]
    public async Task TearDown() => await _league.DisposeAsync();

    // ── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a second draft on the fixture league and returns the updated snapshot.
    /// Adds <paramref name="additionalStandardGames"/> to the league's slot count so the
    /// draft count validation passes.
    /// </summary>
    private async Task<LeagueYearViewModel> CreateSecondDraftAsync(
        string name = "Draft 2",
        int gamesToDraft = 2,
        int counterPicksToDraft = 0,
        int additionalStandardGames = 2,
        int additionalCounterPicks = 0)
    {
        await _league.Manager.LeagueManager.CreateLeagueDraftAsync(new CreateLeagueDraftRequest
        {
            LeagueID = _league.LeagueID,
            Year = _league.Year,
            Name = name,
            ScheduledDate = null,
            GamesToDraft = gamesToDraft,
            CounterPicksToDraft = counterPicksToDraft,
            AdditionalStandardGames = additionalStandardGames,
            AdditionalCounterPicks = additionalCounterPicks,
            NewSpecialSlots = new List<SpecialGameSlotViewModel>(),
        });

        return await _league.GetLeagueYearAsync();
    }

    // ── CreateLeagueDraft ────────────────────────────────────────────────────

    [Test]
    public async Task CreateLeagueDraft_AddsSecondDraft()
    {
        var snapshot = await CreateSecondDraftAsync("Winter Draft", gamesToDraft: 2);

        Assert.That(snapshot.Drafts.Count, Is.GreaterThanOrEqualTo(2),
            "A second draft should appear in the Drafts list.");
        var second = snapshot.Drafts.FirstOrDefault(d => d.Name == "Winter Draft");
        Assert.That(second, Is.Not.Null, "The newly created draft should be visible by name.");
        Assert.That(second!.DraftNumber, Is.EqualTo(2));
        Assert.That(second.GamesToDraft, Is.EqualTo(2));
        Assert.That(second.CounterPicksToDraft, Is.EqualTo(0));
        Assert.That(second.PlayStatus, Is.EqualTo("NotStartedDraft"));
        Assert.That(second.DraftOrderSet, Is.False);
    }

    [Test]
    public async Task CreateLeagueDraft_WithAdditionalStandardGames_IncreasesLeagueSlotCount()
    {
        var before = await _league.GetLeagueYearAsync();
        var originalStandardGames = before.Settings.StandardGames;

        await CreateSecondDraftAsync("Expansion Draft", additionalStandardGames: 3);

        var after = await _league.GetLeagueYearAsync();
        Assert.That(after.Settings.StandardGames, Is.EqualTo(originalStandardGames + 3));
    }

    // ── EditLeagueDraft ──────────────────────────────────────────────────────

    [Test]
    public async Task EditLeagueDraft_Name_UpdatesSuccessfully()
    {
        var after = await CreateSecondDraftAsync("Temp Draft");
        var draft = after.Drafts.First(d => d.Name == "Temp Draft");

        await _league.Manager.LeagueManager.EditLeagueDraftAsync(new EditLeagueDraftRequest
        {
            DraftID = draft.DraftID,
            LeagueID = _league.LeagueID,
            Year = _league.Year,
            Name = "Renamed Draft",
            ScheduledDate = null,
            GamesToDraft = draft.GamesToDraft,
            CounterPicksToDraft = draft.CounterPicksToDraft,
        });

        var updated = await _league.GetLeagueYearAsync();
        var renamedDraft = updated.Drafts.Single(d => d.DraftID == draft.DraftID);
        Assert.That(renamedDraft.Name, Is.EqualTo("Renamed Draft"));
    }

    [Test]
    public async Task EditLeagueDraft_ScheduledDate_UpdatesSuccessfully()
    {
        var after = await CreateSecondDraftAsync("Date Draft");
        var draft = after.Drafts.First(d => d.Name == "Date Draft");

        var targetDate = new DateTimeOffset(2026, 12, 25, 0, 0, 0, TimeSpan.Zero);

        await _league.Manager.LeagueManager.EditLeagueDraftAsync(new EditLeagueDraftRequest
        {
            DraftID = draft.DraftID,
            LeagueID = _league.LeagueID,
            Year = _league.Year,
            Name = draft.Name,
            ScheduledDate = targetDate,
            GamesToDraft = draft.GamesToDraft,
            CounterPicksToDraft = draft.CounterPicksToDraft,
        });

        var updated = await _league.GetLeagueYearAsync();
        var editedDraft = updated.Drafts.Single(d => d.DraftID == draft.DraftID);
        Assert.That(editedDraft.ScheduledDate, Is.Not.Null);
        Assert.That(editedDraft.ScheduledDate!.Value.Date, Is.EqualTo(targetDate.Date));
    }

    [Test]
    public async Task EditLeagueDraft_GamesToDraft_UpdatesSuccessfully()
    {
        var after = await CreateSecondDraftAsync("Count Draft", gamesToDraft: 1, additionalStandardGames: 3);
        var draft = after.Drafts.First(d => d.Name == "Count Draft");

        await _league.Manager.LeagueManager.EditLeagueDraftAsync(new EditLeagueDraftRequest
        {
            DraftID = draft.DraftID,
            LeagueID = _league.LeagueID,
            Year = _league.Year,
            Name = draft.Name,
            ScheduledDate = null,
            GamesToDraft = 2,
            CounterPicksToDraft = draft.CounterPicksToDraft,
        });

        var updated = await _league.GetLeagueYearAsync();
        var editedDraft = updated.Drafts.Single(d => d.DraftID == draft.DraftID);
        Assert.That(editedDraft.GamesToDraft, Is.EqualTo(2));
    }

    // ── DeleteLeagueDraft ────────────────────────────────────────────────────

    [Test]
    public async Task DeleteLeagueDraft_NotStartedSecondDraft_RemovesDraft()
    {
        var after = await CreateSecondDraftAsync("To Delete");
        var draftToDelete = after.Drafts.First(d => d.Name == "To Delete");
        var countBefore = after.Drafts.Count;

        await _league.Manager.LeagueManager.DeleteLeagueDraftAsync(new DeleteLeagueDraftRequest
        {
            DraftID = draftToDelete.DraftID,
            LeagueID = _league.LeagueID,
            Year = _league.Year,
        });

        var updated = await _league.GetLeagueYearAsync();
        Assert.That(updated.Drafts.Count, Is.EqualTo(countBefore - 1));
        Assert.That(updated.Drafts.Any(d => d.DraftID == draftToDelete.DraftID), Is.False,
            "The deleted draft should no longer appear in the list.");
    }

    [Test]
    public async Task DeleteLeagueDraft_FirstDraft_ReturnsBadRequest()
    {
        var snapshot = await _league.GetLeagueYearAsync();
        var firstDraft = snapshot.Drafts.Single(d => d.DraftNumber == 1);

        ApiException? ex = null;
        try
        {
            await _league.Manager.LeagueManager.DeleteLeagueDraftAsync(new DeleteLeagueDraftRequest
            {
                DraftID = firstDraft.DraftID,
                LeagueID = _league.LeagueID,
                Year = _league.Year,
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null, "Deleting the first draft should throw an ApiException.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }

    // ── LeagueYearViewModel always-multi-draft invariant ────────────────────

    [Test]
    public async Task GetLeagueYear_SingleDraftLeague_DraftsListHasExactlyOneEntry()
    {
        // A brand-new Standard league always has exactly one draft (the initial one).
        var snapshot = await _league.GetLeagueYearAsync();
        Assert.That(snapshot.Drafts, Is.Not.Null);
        Assert.That(snapshot.Drafts.Count, Is.EqualTo(1),
            "Single-draft leagues must still expose a one-entry Drafts list.");
        Assert.That(snapshot.Drafts.Single().DraftNumber, Is.EqualTo(1));
        Assert.That(snapshot.Drafts.Single().PlayStatus, Is.EqualTo("NotStartedDraft"));
    }
}
