using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Draft.EdgeCases;

/// <summary>
/// Verifies the manager SkipCurrentDraftPick action and the extended UndoLastDraftAction.
///
/// Two independent fixtures share a common setup helper but each captures its own snapshots.
/// </summary>
[TestFixture]
public class DraftSkipManagerActionTests : IntegrationTestBase
{
    // Fixture A: pause → skip → verify
    private LeagueFixture _skipLeague = null!;
    private LeagueYearViewModel _beforeSkipSnapshot = null!;
    private LeagueYearViewModel _afterSkipSnapshot = null!;

    // Fixture B: pause → skip → undo → verify
    private LeagueFixture _skipUndoLeague = null!;
    private LeagueYearViewModel _beforeSkipUndoSnapshot = null!;
    private LeagueYearViewModel _afterSkipUndoSnapshot = null!;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        // Fixture A -------------------------------------------------------
        _skipLeague = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, LeagueScenarios.TwoPlayerSmall, NewUser);

        await _skipLeague.DraftStandardPicksAsync(1);

        await _skipLeague.Manager.LeagueManager.SetDraftPauseAsync(new DraftPauseRequest
        {
            LeagueID = _skipLeague.LeagueID,
            Year = _skipLeague.Year,
            Pause = true,
        });
        _beforeSkipSnapshot = await _skipLeague.GetLeagueYearAsync();

        var pausedDraftId = _beforeSkipSnapshot.Drafts.Single(x => x.DraftIsPaused).DraftID;

        await _skipLeague.Manager.LeagueManager.SkipCurrentDraftPickAsync(new SkipCurrentDraftPickRequest
        {
            LeagueID = _skipLeague.LeagueID,
            Year = _skipLeague.Year,
            DraftID = pausedDraftId,
        });
        _afterSkipSnapshot = await _skipLeague.GetLeagueYearAsync();

        // Fixture B -------------------------------------------------------
        _skipUndoLeague = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, LeagueScenarios.TwoPlayerSmall, NewUser);

        await _skipUndoLeague.DraftStandardPicksAsync(1);

        await _skipUndoLeague.Manager.LeagueManager.SetDraftPauseAsync(new DraftPauseRequest
        {
            LeagueID = _skipUndoLeague.LeagueID,
            Year = _skipUndoLeague.Year,
            Pause = true,
        });
        var pausedSnapshot2 = await _skipUndoLeague.GetLeagueYearAsync();
        _beforeSkipUndoSnapshot = pausedSnapshot2;
        var pausedDraftId2 = pausedSnapshot2.Drafts.Single(x => x.DraftIsPaused).DraftID;

        await _skipUndoLeague.Manager.LeagueManager.SkipCurrentDraftPickAsync(new SkipCurrentDraftPickRequest
        {
            LeagueID = _skipUndoLeague.LeagueID,
            Year = _skipUndoLeague.Year,
            DraftID = pausedDraftId2,
        });

        await _skipUndoLeague.Manager.LeagueManager.UndoLastDraftActionAsync(new UndoLastDraftActionRequest
        {
            LeagueID = _skipUndoLeague.LeagueID,
            Year = _skipUndoLeague.Year,
            DraftID = pausedDraftId2,
        });
        _afterSkipUndoSnapshot = await _skipUndoLeague.GetLeagueYearAsync();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await _skipLeague.DisposeAsync();
        await _skipUndoLeague.DisposeAsync();
    }

    // ── Fixture A: skip ──────────────────────────────────────────────────

    [Test]
    public void Skip_DraftIsStillPaused()
    {
        Assert.That(_afterSkipSnapshot.ActiveDraft()?.DraftIsPaused ?? false, Is.True,
            "Draft should remain paused after SkipCurrentDraftPick.");
    }

    [Test]
    public void Skip_NextPickPublisherChanges()
    {
        var beforeDraft = _beforeSkipSnapshot.ActiveDraft();
        var afterDraft = _afterSkipSnapshot.ActiveDraft();
        // TwoPlayerSmall snake: after one pick the skipped turn and the following turn can share
        // the same publisher name; round and/or publisher must advance after skip.
        Assert.That(
            afterDraft?.NextPickPublisherName != beforeDraft?.NextPickPublisherName
                || afterDraft?.NextPickRoundNumber != beforeDraft?.NextPickRoundNumber,
            Is.True,
            "Next pick should advance after a skip (publisher and/or round).");
    }

    [Test]
    public async Task Skip_ManagerActionLoggedWithCorrectType()
    {
        var actions = await _skipLeague.Manager.League.GetLeagueActionsAsync(_skipLeague.LeagueID, _skipLeague.Year);
        Assert.That(actions.Any(a => a.ActionType == "Draft Pick Skipped"), Is.True,
            "A 'Draft Pick Skipped' manager action should appear after SkipCurrentDraftPick.");
    }

    // ── Fixture B: skip then undo ─────────────────────────────────────────

    [Test]
    public void SkipUndo_NextPickRestoredToOriginalPublisher()
    {
        var afterUndoPublisher = _afterSkipUndoSnapshot.ActiveDraft()?.NextPickPublisherName;
        var expectedPublisher = _beforeSkipUndoSnapshot.ActiveDraft()?.NextPickPublisherName;
        Assert.That(afterUndoPublisher, Is.EqualTo(expectedPublisher),
            "After undoing the skip the turn should revert to the publisher that was skipped.");
    }

    [Test]
    public async Task SkipUndo_ManagerActionLoggedWithCorrectType()
    {
        var actions = await _skipUndoLeague.Manager.League.GetLeagueActionsAsync(
            _skipUndoLeague.LeagueID, _skipUndoLeague.Year);
        Assert.That(actions.Any(a => a.ActionType == "Draft Skip Undone"), Is.True,
            "'Draft Skip Undone' manager action should appear after undoing a skip.");
    }
}
