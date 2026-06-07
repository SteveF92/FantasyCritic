using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;

namespace FantasyCritic.IntegrationTests.Helpers;

/// <summary>
/// Reusable async helpers for setting up leagues in integration tests.
/// All methods build state through the HTTP API — no direct DB access.
/// </summary>
internal static class LeagueTestHelpers
{
    /// <summary>
    /// Returns the first year currently open for league creation.
    /// Throws if no open years are available (seed DB may be misconfigured).
    /// </summary>
    public static async Task<int> GetOpenYearAsync(ApiSession session)
    {
        var options = await session.League.LeagueOptionsAsync();
        if (options.OpenYears.Count == 0)
            throw new InvalidOperationException(
                "LeagueOptions returned no open years. Is the seed DB running?");
        return options.OpenYears.First();
    }

    /// <summary>
    /// Creates a league under <paramref name="managerSession"/> using the given
    /// <paramref name="scenario"/> settings and returns the new league's GUID.
    /// </summary>
    public static async Task<Guid> CreateLeagueAsync(
        ApiSession managerSession,
        LeagueScenario scenario,
        int year)
    {
        var leagueID = await managerSession.LeagueManager.CreateLeagueAsync(
            new CreateLeagueRequest
            {
                LeagueName = $"TestLeague-{Guid.NewGuid():N}"[..30],
                PublicLeague = false,
                TestLeague = true,
                CustomRulesLeague = false,
                LeagueYearSettings = scenario.BuildSettings(year),
            });

        return leagueID;
    }
}
