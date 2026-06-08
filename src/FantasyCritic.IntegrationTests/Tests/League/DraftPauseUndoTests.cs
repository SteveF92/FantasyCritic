using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League;

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
    private ApiSession _managerSession = null!;
    private ApiSession _playerSession = null!;
    private Guid _leagueID;
    private int _year;
    private Guid _p1PublisherID;

    private LeagueYearViewModel _pausedSnapshot = null!;
    private LeagueYearViewModel _afterUndoSnapshot = null!;
    private LeagueYearViewModel _resumedSnapshot = null!;

    [OneTimeSetUp]
    public async Task SetUpPauseUndoState()
    {
        var (mgrEmail, mgrPassword, mgrDisplayName) = NewUser();
        _managerSession = new ApiSession(Factory);
        await _managerSession.RegisterAsync(mgrEmail, mgrPassword, mgrDisplayName);

        var (plrEmail, plrPassword, plrDisplayName) = NewUser();
        _playerSession = new ApiSession(Factory);
        await _playerSession.RegisterAsync(plrEmail, plrPassword, plrDisplayName);

        IReadOnlyList<Guid> publisherIDs;
        (_leagueID, _year, publisherIDs, _) =
            await LeagueTestHelpers.SetUpLeagueAndStartDraftAsync(
                _managerSession, [_playerSession], LeagueScenarios.TwoPlayerSmall);

        _p1PublisherID = publisherIDs[0]; // manager, draft position 1

        // P1 makes one standard pick so there is a draft action to undo later.
        var p1Player = new MockedLivePlayer(_managerSession, _p1PublisherID, _leagueID);
        await p1Player.DraftStandardGameAsync(_year);

        // ── Pause ──
        await _managerSession.LeagueManager.SetDraftPauseAsync(new DraftPauseRequest
        {
            LeagueID = _leagueID,
            Year = _year,
            Pause = true,
        });
        _pausedSnapshot = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);

        // ── Undo (requires draft to be paused — 400 if active) ──
        await _managerSession.LeagueManager.UndoLastDraftActionAsync(new UndoLastDraftActionRequest
        {
            LeagueID = _leagueID,
            Year = _year,
        });
        _afterUndoSnapshot = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);

        // ── Resume ──
        await _managerSession.LeagueManager.SetDraftPauseAsync(new DraftPauseRequest
        {
            LeagueID = _leagueID,
            Year = _year,
            Pause = false,
        });
        _resumedSnapshot = await _managerSession.League.GetLeagueYearAsync(_leagueID, _year, null);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _managerSession?.Dispose();
        _playerSession?.Dispose();
    }

    // ── Paused snapshot assertions ─────────────────────────────────────────────

    [Test]
    public void Pause_DraftIsNotActive()
    {
        Assert.That(_pausedSnapshot.PlayStatus.DraftIsActive, Is.False,
            "DraftIsActive must be false while the draft is paused.");
    }

    [Test]
    public void Pause_DraftIsNotFinished()
    {
        Assert.That(_pausedSnapshot.PlayStatus.DraftFinished, Is.False,
            "DraftFinished must be false while the draft is paused mid-draft.");
    }

    [Test]
    public void Pause_NoPublisherIsNextToDraft()
    {
        // NextToDraft is derived from DraftFunctions.GetDraftStatus, which returns null when
        // the draft is paused (no active turn). If this fails, check whether the server actually
        // sets NextToDraft on a paused draft and update the assertion to match.
        Assert.That(
            _pausedSnapshot.Publishers.Any(p => p.NextToDraft),
            Is.False,
            "No publisher should be marked NextToDraft while the draft is paused.");
    }

    // ── After-undo snapshot assertions (still paused) ──────────────────────────

    [Test]
    public void Undo_P1HasNoGames()
    {
        var p1 = _afterUndoSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID);
        Assert.That(p1.Games, Is.Empty,
            "P1's game should be removed after UndoLastDraftAction.");
    }

    [Test]
    public void Undo_NoPublisherIsNextToDraft()
    {
        Assert.That(
            _afterUndoSnapshot.Publishers.Any(p => p.NextToDraft),
            Is.False,
            "No publisher should be NextToDraft while the draft is still paused after undo.");
    }

    // ── Resumed snapshot assertions ────────────────────────────────────────────

    [Test]
    public void Resume_DraftIsActive()
    {
        Assert.That(_resumedSnapshot.PlayStatus.DraftIsActive, Is.True,
            "DraftIsActive must be true after resuming the draft.");
    }

    [Test]
    public void Resume_P1IsNextToDraft()
    {
        // Undo reverted P1's pick, restoring them to the head of the draft order with 0 games.
        // After resume, P1 (draft position 1) should be next again.
        var p1 = _resumedSnapshot.Publishers.Single(p => p.PublisherID == _p1PublisherID);
        Assert.That(p1.NextToDraft, Is.True,
            "P1 should be NextToDraft after resume: undo put them back at position 1 with 0 games.");
    }
}
