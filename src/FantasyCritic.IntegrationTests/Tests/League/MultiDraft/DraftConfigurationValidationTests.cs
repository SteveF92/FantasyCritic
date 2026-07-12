using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.MultiDraft;

/// <summary>
/// Verifies the server-side validation added to <c>ValidateDrafts</c> in
/// <c>DraftFunctions</c> and the call sites in <c>FantasyCriticService</c> and
/// <c>DraftService</c>.
///
/// Configuration-time tests (Create/Edit) use a shared pre-draft fixture.
/// Start-time tests create isolated leagues so they can run Draft 1 to completion.
/// </summary>
[TestFixture]
public class DraftConfigurationValidationTests : IntegrationTestBase
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

    // ── CreateLeagueDraft ────────────────────────────────────────────────────

    [Test]
    public async Task CreateLeagueDraft_ZeroGamesAndCounterPicks_ReturnsBadRequest()
    {
        ApiException? ex = null;
        try
        {
            await _league.Manager.LeagueManager.CreateLeagueDraftAsync(new CreateLeagueDraftRequest
            {
                LeagueID = _league.LeagueID,
                Year = _league.Year,
                Name = "Zero Draft",
                ScheduledDate = null,
                GamesToDraft = 0,
                CounterPicksToDraft = 0,
                AdditionalStandardGames = 0,
                AdditionalCounterPicks = 0,
                NewSpecialGameSlots = [],
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null, "A draft with zero games and zero counter picks should be rejected.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateLeagueDraft_CounterPicksMustBeFromThisDraft_MoreCounterPicksThanGames_ReturnsBadRequest()
    {
        ApiException? ex = null;
        try
        {
            await _league.Manager.LeagueManager.CreateLeagueDraftAsync(new CreateLeagueDraftRequest
            {
                LeagueID = _league.LeagueID,
                Year = _league.Year,
                Name = "Bad CP Draft",
                ScheduledDate = null,
                GamesToDraft = 1,
                CounterPicksToDraft = 2,
                CounterPicksMustBeFromThisDraft = true,
                AdditionalStandardGames = 3,
                AdditionalCounterPicks = 3,
                NewSpecialGameSlots = [],
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null,
            "CounterPicksMustBeFromThisDraft=true with more counter picks than games should be rejected.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }

    // ── EditLeagueDraft ──────────────────────────────────────────────────────

    [Test]
    public async Task EditLeagueDraft_ZeroGamesAndCounterPicks_ReturnsBadRequest()
    {
        await _league.Manager.LeagueManager.CreateLeagueDraftAsync(new CreateLeagueDraftRequest
        {
            LeagueID = _league.LeagueID,
            Year = _league.Year,
            Name = "Edit Zero Target",
            ScheduledDate = null,
            GamesToDraft = 2,
            CounterPicksToDraft = 0,
            AdditionalStandardGames = 2,
            AdditionalCounterPicks = 0,
            NewSpecialGameSlots = [],
        });

        var snapshot = await _league.GetLeagueYearAsync();
        var draft = snapshot.Drafts.First(d => d.Name == "Edit Zero Target");

        ApiException? ex = null;
        try
        {
            await _league.Manager.LeagueManager.EditLeagueDraftAsync(new EditLeagueDraftRequest
            {
                DraftID = draft.DraftID,
                LeagueID = _league.LeagueID,
                Year = _league.Year,
                Name = draft.Name,
                ScheduledDate = null,
                GamesToDraft = 0,
                CounterPicksToDraft = 0,
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null, "Editing a draft to zero games and zero counter picks should be rejected.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task EditLeagueDraft_CounterPicksMustBeFromThisDraft_MoreCounterPicksThanGames_ReturnsBadRequest()
    {
        await _league.Manager.LeagueManager.CreateLeagueDraftAsync(new CreateLeagueDraftRequest
        {
            LeagueID = _league.LeagueID,
            Year = _league.Year,
            Name = "Edit CP Target",
            ScheduledDate = null,
            GamesToDraft = 2,
            CounterPicksToDraft = 1,
            AdditionalStandardGames = 2,
            AdditionalCounterPicks = 2,
            NewSpecialGameSlots = [],
        });

        var snapshot = await _league.GetLeagueYearAsync();
        var draft = snapshot.Drafts.First(d => d.Name == "Edit CP Target");

        ApiException? ex = null;
        try
        {
            await _league.Manager.LeagueManager.EditLeagueDraftAsync(new EditLeagueDraftRequest
            {
                DraftID = draft.DraftID,
                LeagueID = _league.LeagueID,
                Year = _league.Year,
                Name = draft.Name,
                ScheduledDate = null,
                GamesToDraft = 1,
                CounterPicksToDraft = 2,
                CounterPicksMustBeFromThisDraft = true,
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null,
            "Editing to CounterPicksMustBeFromThisDraft=true with more counter picks than games should be rejected.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }

    // ── Start-time ───────────────────────────────────────────────────────────
    //
    // FourPlayerBidding: 6 standard slots, 3 to draft, 2 counter-pick slots, 1 CP to draft.
    // After Draft 1 completes: remainingStdSlots = 6 - 3 = 3, remainingCPSlots = 2 - 1 = 1.
    // Publishers have 3/6 and 1/2 slots filled, so the pre-existing "all slots full" publisher
    // check does NOT fire — only our new ValidateDrafts start-time check does.

    [Test]
    public async Task StartDraft2_GamesToDraftExceedsRemainingStandardSlots_DraftNotReady()
    {
        await using var league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, LeagueScenarios.FourPlayerBidding, NewUser);
        await league.DraftToCompletionAsync();

        // remainingStdSlots = 6 - 3 = 3; configure Draft 2 to draft 4 (exceeds remaining)
        await league.Manager.LeagueManager.CreateLeagueDraftAsync(new CreateLeagueDraftRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
            Name = "Overflow Draft",
            ScheduledDate = null,
            GamesToDraft = 4,
            CounterPicksToDraft = 0,
            AdditionalStandardGames = 0,
            AdditionalCounterPicks = 0,
            NewSpecialGameSlots = [],
        });

        await MultiDraftTestScenario.SetDraft2OrderAsync(league);

        var snapshot = await league.GetLeagueYearAsync();
        var draft2 = snapshot.Drafts.Single(d => d.DraftNumber == 2);

        Assert.That(draft2.ReadyToDraft, Is.False,
            "Draft 2 should not be ready when GamesToDraft exceeds remaining standard slots.");
        Assert.That(draft2.StartDraftErrors, Has.Some.Contains("remaining"),
            "Start errors should explain that there are not enough remaining standard slots.");
    }

    [Test]
    public async Task StartDraft2_CounterPicksToDraftExceedsRemainingCounterPickSlots_DraftNotReady()
    {
        await using var league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, LeagueScenarios.FourPlayerBidding, NewUser);
        await league.DraftToCompletionAsync();

        // remainingCPSlots = 2 - 1 = 1; configure Draft 2 to draft 2 counter picks (exceeds remaining)
        await league.Manager.LeagueManager.CreateLeagueDraftAsync(new CreateLeagueDraftRequest
        {
            LeagueID = league.LeagueID,
            Year = league.Year,
            Name = "Overflow CP Draft",
            ScheduledDate = null,
            GamesToDraft = 1,
            CounterPicksToDraft = 2,
            AdditionalStandardGames = 1,
            AdditionalCounterPicks = 0,
            NewSpecialGameSlots = [],
        });

        await MultiDraftTestScenario.SetDraft2OrderAsync(league);

        var snapshot = await league.GetLeagueYearAsync();
        var draft2 = snapshot.Drafts.Single(d => d.DraftNumber == 2);

        Assert.That(draft2.ReadyToDraft, Is.False,
            "Draft 2 should not be ready when CounterPicksToDraft exceeds remaining counter-pick slots.");
        Assert.That(draft2.StartDraftErrors, Has.Some.Contains("remaining"),
            "Start errors should explain that there are not enough remaining counter-pick slots.");
    }
}
