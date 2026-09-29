using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Utilities;

internal static class PublicBiddingJobUtilities
{
    public static async Task<IReadOnlyList<LeagueYearPublicBiddingSet>> GetPublicBiddingSets(
        IFantasyCriticRepo fantasyCriticRepo, GameAcquisitionService gameAcquisitionService)
    {
        var supportedYears = await fantasyCriticRepo.GetSupportedYears();
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
