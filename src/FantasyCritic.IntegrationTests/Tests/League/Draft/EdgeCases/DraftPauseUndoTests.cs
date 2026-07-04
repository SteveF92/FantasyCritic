using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Draft.EdgeCases;

/// <summary>
/// Verifies the SetDraftPause → UndoLastDraftAction → resume sequence.
///
/// OneTimeSetUp performs the full sequence and captures three snapshots.
/// Tests assert against those snapshots — no test mutates state.
/// Note: UndoLastDraftAction requires <c>RequiredYearStatus.DraftPaused</c>;
/// the pause → undo → resume order in OneTimeSetUp is mandatory.
/// </summary>
[TestFixture]
public class DraftPauseUndoTests : IntegrationTestBase
{
    private LeagueFixture _league = null!;

    private LeagueYearViewModel _pausedSnapshot = null!;
    private LeagueYearViewModel _afterUndoSnapshot = null!;
    private LeagueYearViewModel _resumedSnapshot = null!;

    [OneTimeSetUp]
    public async Task SetUpPauseUndoState()
    {
        _league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, LeagueScenarios.TwoPlayerSmall, NewUser);

        await _league.DraftStandardPicksAsync(1);

        await _league.Manager.LeagueManager.SetDraftPauseAsync(new DraftPauseRequest
        {
            LeagueID = _league.LeagueID,
            Year = _league.Year,
            Pause = true,
        });
        _pausedSnapshot = await _league.GetLeagueYearAsync();

        await _league.Manager.LeagueManager.UndoLastDraftActionAsync(new UndoLastDraftActionRequest
        {
            LeagueID = _league.LeagueID,
            Year = _league.Year,
            DraftID = _pausedSnapshot.Drafts.First(x => x.DraftIsPaused).DraftID
        });
        _afterUndoSnapshot = await _league.GetLeagueYearAsync();

        await _league.Manager.LeagueManager.SetDraftPauseAsync(new DraftPauseRequest
        {
            LeagueID = _league.LeagueID,
            Year = _league.Year,
            Pause = false,
        });
        _resumedSnapshot = await _league.GetLeagueYearAsync();
    }

    [OneTimeTearDown]
    public async Task TearDown() => await _league.DisposeAsync();

    [Test]
    public void Pause_DraftIsNotActive()
    {
        Assert.That(_pausedSnapshot.ActiveDraft()?.DraftIsActive ?? false, Is.False,
            "DraftIsActive must be false while the draft is paused.");
    }

    [Test]
    public void Pause_DraftIsNotFinished()
    {
        Assert.That(_pausedSnapshot.ActiveDraft()?.DraftFinished ?? false, Is.False,
            "DraftFinished must be false while the draft is paused mid-draft.");
    }

    [Test]
    public void Pause_P2IsNextToDraft()
    {
        Assert.That(_pausedSnapshot.ActiveDraft()?.DraftIsPaused ?? false, Is.True,
            "Draft should still be paused in this snapshot.");
        var p2 = _pausedSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[1].PublisherID);
        Assert.That(p2.NextToDraft, Is.True,
            "P2 should still be NextToDraft while paused: P1 made pick 1 and the turn does not clear on pause.");
    }

    [Test]
    public void Undo_P1HasNoGames()
    {
        var p1 = _afterUndoSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[0].PublisherID);
        Assert.That(p1.Games, Is.Empty,
            "P1's game should be removed after UndoLastDraftAction.");
    }

    [Test]
    public void Undo_P1IsNextToDraftWhilePaused()
    {
        Assert.That(_afterUndoSnapshot.ActiveDraft()?.DraftIsPaused ?? false, Is.True,
            "Draft should still be paused after undo.");
        var p1 = _afterUndoSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[0].PublisherID);
        Assert.That(p1.NextToDraft, Is.True,
            "P1 should be NextToDraft while still paused: undo removed their pick and they are back on the clock.");
    }

    [Test]
    public void Resume_DraftIsActive()
    {
        Assert.That(_resumedSnapshot.ActiveDraft()?.DraftIsActive ?? false, Is.True,
            "DraftIsActive must be true after resuming the draft.");
    }

    [Test]
    public void Resume_P1IsNextToDraft()
    {
        var p1 = _resumedSnapshot.Publishers.Single(p => p.PublisherID == _league.Publishers[0].PublisherID);
        Assert.That(p1.NextToDraft, Is.True,
            "P1 should be NextToDraft after resume: undo put them back at position 1 with 0 games.");
    }

    [Test]
    public async Task Undo_LogsDraftPickUndoneActionType()
    {
        var actions = await _league.Manager.League.GetLeagueActionsAsync(_league.LeagueID, _league.Year);
        Assert.That(actions.Any(a => a.ActionType == "Draft Pick Undone"), Is.True,
            "Undo should log 'Draft Pick Undone', not 'Publisher Game Removed'.");
        Assert.That(actions.Any(a => a.ActionType == "Publisher Game Removed"), Is.False,
            "Draft undo should not use the generic game removal action type.");
    }
}
