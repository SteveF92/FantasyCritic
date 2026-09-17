using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal static class PublicBiddingJobUtilities
{
    public static async Task<IReadOnlyList<LeagueYearPublicBiddingSet>> GetPublicBiddingSets(
        InterLeagueService interLeagueService, GameAcquisitionService gameAcquisitionService)
    {
        var supportedYears = await interLeagueService.GetSupportedYears();
        var activeYears = supportedYears.Where(x => x.OpenForPlay && !x.Finished);

        var publicBiddingSets = new List<LeagueYearPublicBiddingSet>();
        foreach (var year in activeYears)
        {
            var publicBiddingSetsForYear = await gameAcquisitionService.GetPublicBiddingGames(year.Year);
            publicBiddingSets.AddRange(publicBiddingSetsForYear);
        }

        return publicBiddingSets;
    }
}
