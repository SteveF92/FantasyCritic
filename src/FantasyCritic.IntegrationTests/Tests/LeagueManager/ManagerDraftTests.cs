using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using FantasyCritic.IntegrationTests.Tests.League;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.LeagueManager;

/// <summary>
/// Exercises a full draft using the manager-side <c>ManagerDraftGame</c> endpoint.
/// Inherits all shared post-draft assertions from <see cref="LeagueDraftTestBase"/>.
/// Uses the same <see cref="LeagueScenarios.Standard"/> scenario as
/// <see cref="Scenarios.StandardLeagueDraftTests"/> so both endpoints are verified to
/// produce the same outcome.
/// </summary>
[TestFixture]
public class ManagerDraftTests : LeagueDraftTestBase
{
    protected override LeagueScenario Scenario => LeagueScenarios.Standard;

    /// <summary>
    /// Overrides the default player-side draft with manager-controlled picks
    /// using <c>ManagerDraftGame</c>. The manager session polls league-year state
    /// and makes every pick on behalf of whoever is next.
    /// </summary>
    protected override async Task SimulateDraftAsync(
        IReadOnlyDictionary<Guid, ApiSession> publisherSessionMap,
        Guid leagueID,
        int year)
    {
        while (true)
        {
            var leagueYear = await ManagerSession.League.GetLeagueYearAsync(leagueID, year, null);

            if (leagueYear.PlayStatus.DraftFinished)
                return;

            var nextPublisher = leagueYear.Publishers.SingleOrDefault(p => p.NextToDraft)
                ?? throw new InvalidOperationException(
                    "Draft is active and not finished, but no publisher has NextToDraft = true.");

            if (leagueYear.PlayStatus.DraftingCounterPicks)
            {
                var options = await ManagerSession.League.PossibleCounterPicksAsync(
                    nextPublisher.PublisherID);

                var pick = options.FirstOrDefault()
                    ?? throw new InvalidOperationException(
                        $"PossibleCounterPicks returned no options for publisher {nextPublisher.PublisherID}.");

                var result = await ManagerSession.LeagueManager.ManagerDraftGameAsync(
                    new ManagerDraftGameRequest
                    {
                        PublisherID = nextPublisher.PublisherID,
                        MasterGameID = pick.MasterGame!.MasterGameID,
                        GameName = pick.GameName,
                        CounterPick = true,
                        ManagerOverride = false,
                        AllowIneligibleSlot = false,
                    });

                if (!result.Success)
                    throw new InvalidOperationException(
                        $"ManagerDraftGame (counter-pick) failed for publisher {nextPublisher.PublisherID}: "
                        + string.Join("; ", result.Errors ?? []));
            }
            else
            {
                var available = await ManagerSession.League.TopAvailableGamesAsync(
                    year, leagueID, nextPublisher.PublisherID, slotInfo: null);

                var game = available.FirstOrDefault(g => g.IsAvailable && !g.Taken)
                    ?? throw new InvalidOperationException(
                        $"TopAvailableGames returned no available games for publisher {nextPublisher.PublisherID}.");

                var result = await ManagerSession.LeagueManager.ManagerDraftGameAsync(
                    new ManagerDraftGameRequest
                    {
                        PublisherID = nextPublisher.PublisherID,
                        MasterGameID = game.MasterGame.MasterGameID,
                        GameName = game.MasterGame.GameName,
                        CounterPick = false,
                        ManagerOverride = false,
                        AllowIneligibleSlot = false,
                    });

                if (!result.Success)
                    throw new InvalidOperationException(
                        $"ManagerDraftGame (standard) failed for publisher {nextPublisher.PublisherID}: "
                        + string.Join("; ", result.Errors ?? []));
            }
        }
    }
}
