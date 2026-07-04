using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Draft.Scenarios;

/// <summary>
/// Exercises a full draft using the manager-side <c>ManagerDraftGame</c> endpoint.
/// Inherits all shared post-draft assertions from <see cref="LeagueDraftTestBase"/>.
/// Uses the same <see cref="LeagueScenarios.Standard"/> scenario as
/// <see cref="StandardLeagueDraftTests"/> so both endpoints are verified to
/// produce the same outcome.
/// </summary>
[TestFixture]
public class ManagerDraftTests : LeagueDraftTestBase
{
    protected override LeagueScenario Scenario => LeagueScenarios.Standard;

    protected override async Task SimulateDraftAsync()
    {
        while (true)
        {
            var leagueYear = await League.GetLeagueYearAsync();

            bool anyDraftActive = leagueYear.Drafts.Any(d =>
                d.PlayStatus == "Drafting" || d.PlayStatus == "DraftPaused");
            if (!anyDraftActive)
                return;

            var nextPublisher = leagueYear.Publishers.SingleOrDefault(p => p.NextToDraft)
                ?? throw new InvalidOperationException(
                    "Draft is active and not finished, but no publisher has NextToDraft = true.");

            if (leagueYear.ActiveDraft()?.DraftingCounterPicks == true)
            {
                var options = await League.Manager.League.PossibleCounterPicksAsync(
                    nextPublisher.PublisherID);

                var pick = options.FirstOrDefault()
                    ?? throw new InvalidOperationException(
                        $"PossibleCounterPicks returned no options for publisher {nextPublisher.PublisherID}.");

                var result = await League.Manager.LeagueManager.ManagerDraftGameAsync(
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
                var available = await League.Manager.League.TopAvailableGamesAsync(
                    League.Year, League.LeagueID, nextPublisher.PublisherID, slotInfo: null);

                var game = available.FirstOrDefault(g => g.IsAvailable && !g.Taken)
                    ?? throw new InvalidOperationException(
                        $"TopAvailableGames returned no available games for publisher {nextPublisher.PublisherID}.");

                var result = await League.Manager.LeagueManager.ManagerDraftGameAsync(
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
