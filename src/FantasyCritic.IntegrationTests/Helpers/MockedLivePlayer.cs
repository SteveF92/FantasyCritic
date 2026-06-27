using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;

namespace FantasyCritic.IntegrationTests.Helpers;

/// <summary>
/// Simulates a single live player making picks during a draft.
/// Dynamically selects the first available eligible game each turn via the API
/// (same strategy as the site's own game-search UI).
/// Override <see cref="DraftStandardGameAsync"/> or <see cref="DraftCounterPickAsync"/>
/// to inject custom pick logic in scenario-specific tests.
/// </summary>
internal class MockedLivePlayer
{
    private readonly ApiSession _session;

    public Guid PublisherID { get; }
    public Guid LeagueID { get; }

    public MockedLivePlayer(ApiSession session, Guid publisherID, Guid leagueID)
    {
        _session = session;
        PublisherID = publisherID;
        LeagueID = leagueID;
    }

    /// <summary>
    /// Picks the first available, un-taken eligible game from TopAvailableGames.
    /// Iterates past any games the server rejects (e.g. games that gained a score
    /// from a prior test run and persist in the DB).
    /// </summary>
    public virtual async Task DraftStandardGameAsync(int year)
    {
        var available = await _session.League.TopAvailableGamesAsync(
            year, LeagueID, PublisherID, slotInfo: null);

        var candidates = available.Where(g => g.IsAvailable && !g.Taken).ToList();
        if (candidates.Count == 0)
            throw new InvalidOperationException(
                $"TopAvailableGames returned no available games for publisher {PublisherID}.");

        foreach (var pick in candidates)
        {
            var result = await _session.League.DraftGameAsync(new DraftGameRequest
            {
                PublisherID = PublisherID,
                MasterGameID = pick.MasterGame.MasterGameID,
                GameName = pick.MasterGame.GameName,
                CounterPick = false,
                AllowIneligibleSlot = false,
            });

            if (result.Success)
                return;
        }

        throw new InvalidOperationException(
            $"DraftGame (standard) failed for publisher {PublisherID}: "
            + "no candidate from TopAvailableGames could be successfully drafted.");
    }

    /// <summary>
    /// Picks the first available counter-pick from PossibleCounterPicks.
    /// </summary>
    public virtual async Task DraftCounterPickAsync(int year)
    {
        var options = await _session.League.PossibleCounterPicksAsync(PublisherID);

        var pick = options.FirstOrDefault()
            ?? throw new InvalidOperationException(
                $"PossibleCounterPicks returned no options for publisher {PublisherID}.");

        var result = await _session.League.DraftGameAsync(new DraftGameRequest
        {
            PublisherID = PublisherID,
            MasterGameID = pick.MasterGame!.MasterGameID,
            GameName = pick.GameName,
            CounterPick = true,
            AllowIneligibleSlot = false,
        });

        if (!result.Success)
            throw new InvalidOperationException(
                $"DraftGame (counter-pick) failed for publisher {PublisherID}: "
                + string.Join("; ", result.Errors));
    }
}

/// <summary>
/// Drives the draft loop for a full league draft.
/// Polls <see cref="ApiSession.League"/>.GetLeagueYear to find the next publisher,
/// delegates to the correct <see cref="MockedLivePlayer"/>, and returns when the
/// draft is finished.
/// </summary>
internal sealed class DraftSimulator
{
    private readonly ApiSession _observerSession;
    private readonly IReadOnlyDictionary<Guid, MockedLivePlayer> _players;

    /// <param name="observerSession">
    ///   Any authenticated session used only to poll league-year state.
    ///   Typically the manager session.
    /// </param>
    /// <param name="players">One <see cref="MockedLivePlayer"/> per publisher.</param>
    public DraftSimulator(ApiSession observerSession, IEnumerable<MockedLivePlayer> players)
    {
        _observerSession = observerSession;
        _players = players.ToDictionary(p => p.PublisherID);
    }

    /// <summary>
    /// Runs the draft to completion. Throws if a publisher is expected to pick
    /// but has no entry in the player map.
    /// </summary>
    public async Task RunAsync(Guid leagueID, int year)
    {
        while (true)
        {
            var leagueYear = await _observerSession.League.GetLeagueYearAsync(leagueID, year, null);

            // Stop when no draft is actively running (handles both single-draft and multi-draft).
            // PlayStatus.DraftFinished can be misleading in multi-draft leagues because it reflects
            // only the first draft; instead, check the per-draft statuses directly.
            bool anyDraftActive = leagueYear.Drafts.Any(d =>
                d.PlayStatus == "Drafting" || d.PlayStatus == "DraftPaused");
            if (!anyDraftActive)
                return;

            var nextPublisher = leagueYear.Publishers.SingleOrDefault(p => p.NextToDraft)
                ?? throw new InvalidOperationException(
                    "Draft is active and not finished, but no publisher has NextToDraft = true.");

            if (!_players.TryGetValue(nextPublisher.PublisherID, out var player))
                throw new InvalidOperationException(
                    $"No MockedLivePlayer registered for publisher {nextPublisher.PublisherID} "
                    + $"({nextPublisher.PublisherName}). "
                    + "If this publisher has auto-draft enabled, their auto-draft may have failed "
                    + "(e.g. no available games found for the slot). "
                    + "Check TopAvailableGames for this publisher.");

            if (leagueYear.ActiveDraft()?.DraftingCounterPicks == true)
                await player.DraftCounterPickAsync(year);
            else
                await player.DraftStandardGameAsync(year);
        }
    }

    /// <summary>
    /// Runs exactly <paramref name="count"/> standard-game picks, then returns.
    /// </summary>
    public async Task RunStandardPickCountAsync(Guid leagueID, int year, int count)
    {
        for (var pick = 0; pick < count; pick++)
        {
            var leagueYear = await _observerSession.League.GetLeagueYearAsync(leagueID, year, null);

            var activeDraft = leagueYear.ActiveDraft();
            if (activeDraft is null || activeDraft.DraftFinished || activeDraft.DraftingCounterPicks)
                throw new InvalidOperationException(
                    $"Cannot run standard pick {pick + 1} of {count}: draft is finished or in counter-pick phase.");

            var nextPublisher = leagueYear.Publishers.SingleOrDefault(p => p.NextToDraft)
                ?? throw new InvalidOperationException(
                    "Draft is active but no publisher has NextToDraft = true.");

            if (!_players.TryGetValue(nextPublisher.PublisherID, out var player))
                throw new InvalidOperationException(
                    $"No MockedLivePlayer registered for publisher {nextPublisher.PublisherID} "
                    + $"({nextPublisher.PublisherName}).");

            await player.DraftStandardGameAsync(year);
        }
    }

    /// <summary>
    /// Runs the draft loop until the counter-pick phase begins, then returns without picking
    /// any counter-picks. Use this to set up shared state for counter-pick-phase error tests.
    /// </summary>
    public async Task RunUntilCounterPickPhaseAsync(Guid leagueID, int year)
    {
        while (true)
        {
            var leagueYear = await _observerSession.League.GetLeagueYearAsync(leagueID, year, null);

            var activeDraft = leagueYear.ActiveDraft();
            if (activeDraft is null || activeDraft.DraftFinished || activeDraft.DraftingCounterPicks)
                return;

            var nextPublisher = leagueYear.Publishers.SingleOrDefault(p => p.NextToDraft)
                ?? throw new InvalidOperationException(
                    "Draft is active, not finished, and not yet in the counter-pick phase, "
                    + "but no publisher has NextToDraft = true.");

            if (!_players.TryGetValue(nextPublisher.PublisherID, out var player))
                throw new InvalidOperationException(
                    $"No MockedLivePlayer registered for publisher {nextPublisher.PublisherID} "
                    + $"({nextPublisher.PublisherName}).");

            await player.DraftStandardGameAsync(year);
        }
    }
}
