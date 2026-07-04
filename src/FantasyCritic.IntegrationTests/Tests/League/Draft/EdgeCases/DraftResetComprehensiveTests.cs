using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Draft.EdgeCases;

/// <summary>
/// Exercises a full mid-draft lifecycle: picks, undo pick, more picks, skips, undo skip, reset.
/// Verifies reset clears drafted games and stored pick skips.
/// </summary>
[TestFixture]
public class DraftResetComprehensiveTests : IntegrationTestBase
{
    private LeagueFixture _league = null!;

    private Guid _draftID;
    private string _publisherSkippedBeforeReset = null!;
    private LeagueYearViewModel _beforeFirstSkipSnapshot = null!;
    private LeagueYearViewModel _afterFirstSkipSnapshot = null!;
    private LeagueYearViewModel _afterResetSnapshot = null!;
    private LeagueYearViewModel _afterRestartSnapshot = null!;

    [OneTimeSetUp]
    public async Task SetUpFullDraftLifecycle()
    {
        _league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(
            Factory, LeagueScenarios.TwoPlayerSmall, NewUser);

        _draftID = (await _league.GetLeagueYearAsync()).Drafts.Single().DraftID;

        await _league.DraftStandardPicksAsync(2);

        await SetDraftPauseAsync(true);
        await _league.Manager.LeagueManager.UndoLastDraftActionAsync(new UndoLastDraftActionRequest
        {
            LeagueID = _league.LeagueID,
            Year = _league.Year,
            DraftID = _draftID,
        });

        await SetDraftPauseAsync(false);
        await _league.DraftStandardPicksAsync(1);

        await SetDraftPauseAsync(true);
        _beforeFirstSkipSnapshot = await _league.GetLeagueYearAsync();
        _publisherSkippedBeforeReset = _beforeFirstSkipSnapshot.ActiveDraft()!.NextPickPublisherName!;

        await _league.Manager.LeagueManager.SkipCurrentDraftPickAsync(new SkipCurrentDraftPickRequest
        {
            LeagueID = _league.LeagueID,
            Year = _league.Year,
            DraftID = _draftID,
        });
        _afterFirstSkipSnapshot = await _league.GetLeagueYearAsync();

        await SetDraftPauseAsync(false);
        await _league.DraftStandardPicksAsync(1);

        await SetDraftPauseAsync(true);
        await _league.Manager.LeagueManager.SkipCurrentDraftPickAsync(new SkipCurrentDraftPickRequest
        {
            LeagueID = _league.LeagueID,
            Year = _league.Year,
            DraftID = _draftID,
        });

        await _league.Manager.LeagueManager.UndoLastDraftActionAsync(new UndoLastDraftActionRequest
        {
            LeagueID = _league.LeagueID,
            Year = _league.Year,
            DraftID = _draftID,
        });

        await _league.Manager.LeagueManager.ResetDraftAsync(new ResetDraftRequest
        {
            LeagueID = _league.LeagueID,
            Year = _league.Year,
            DraftID = _draftID,
        });
        _afterResetSnapshot = await _league.GetLeagueYearAsync();

        await _league.Manager.LeagueManager.StartDraftAsync(new StartDraftRequest
        {
            LeagueID = _league.LeagueID,
            Year = _league.Year,
        });

        await _league.DraftStandardPicksAsync(2);
        await SetDraftPauseAsync(true);
        _afterRestartSnapshot = await _league.GetLeagueYearAsync();
    }

    [OneTimeTearDown]
    public async Task TearDown() => await _league.DisposeAsync();

    [Test]
    public void FirstSkip_AdvancesNextPick()
    {
        var beforePublisher = _beforeFirstSkipSnapshot.ActiveDraft()?.NextPickPublisherName;
        var afterPublisher = _afterFirstSkipSnapshot.ActiveDraft()?.NextPickPublisherName;
        Assert.That(
            afterPublisher != beforePublisher
                || _afterFirstSkipSnapshot.ActiveDraft()?.NextPickRoundNumber
                    != _beforeFirstSkipSnapshot.ActiveDraft()?.NextPickRoundNumber,
            Is.True,
            "Skip should advance the next pick (publisher and/or round).");
    }

    [Test]
    public void FirstSkip_SkippedPicksSinceLastRealPickReflectsManualSkip()
    {
        var skippedPicks = _afterFirstSkipSnapshot.ActiveDraft()?.SkippedPicksSinceLastRealPick;

        Assert.That(skippedPicks, Is.Not.Null);
        Assert.That(skippedPicks, Has.Count.EqualTo(1));
        var skippedPick = skippedPicks!.Single();
        Assert.That(skippedPick.PublisherName, Is.EqualTo(_publisherSkippedBeforeReset));
        Assert.That(skippedPick.IsManualSkip, Is.True);
    }

    [Test]
    public void Reset_ClearsAllPublisherGames()
    {
        Assert.That(_afterResetSnapshot.Publishers.All(p => p.Games.Count == 0), Is.True,
            "Reset should remove every drafted game for the draft.");
    }

    [Test]
    public void Reset_DraftReturnsToNotStarted()
    {
        var draft = _afterResetSnapshot.Drafts.Single(d => d.DraftID == _draftID);
        Assert.That(draft.PlayStatus, Is.EqualTo("NotStartedDraft"));
        Assert.That(_afterResetSnapshot.ActiveDraft(), Is.Null,
            "No draft should be active immediately after reset.");
    }

    [Test]
    public void Reset_ClearsStoredPickSkips()
    {
        var nextPublisherAfterRestart = _afterRestartSnapshot.ActiveDraft()?.NextPickPublisherName;
        Assert.That(nextPublisherAfterRestart, Is.EqualTo(_publisherSkippedBeforeReset),
            "After reset and replaying to the same point, the previously skipped publisher "
            + "should be on the clock again — stored skips must have been cleared.");
    }

    [Test]
    public async Task Reset_LogsDraftResetAction()
    {
        var actions = await _league.Manager.League.GetLeagueActionsAsync(_league.LeagueID, _league.Year);
        Assert.That(actions.Any(a => a.ActionType == "Draft Reset"), Is.True,
            "Reset should log a 'Draft Reset' manager action.");
    }

    private Task SetDraftPauseAsync(bool pause) =>
        _league.Manager.LeagueManager.SetDraftPauseAsync(new DraftPauseRequest
        {
            LeagueID = _league.LeagueID,
            Year = _league.Year,
            Pause = pause,
        });
}
