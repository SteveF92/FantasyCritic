using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.League.Scenarios;

/// <summary>
/// Exercises a full draft using the player-side <c>DraftGame</c> endpoint.
/// Uses the <see cref="LeagueScenarios.Standard"/> scenario:
/// 4 players, 6 standard + 1 counter-pick, Flexible draft.
/// </summary>
[TestFixture]
public class StandardLeagueDraftTests : LeagueDraftTestBase
{
    protected override LeagueScenario Scenario => LeagueScenarios.Standard;
}
