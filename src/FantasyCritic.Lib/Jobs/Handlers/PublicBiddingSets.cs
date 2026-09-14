using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

//The public bidding emails and the Discord summary are separate jobs, so a failed email send can be re-run without reposting to Discord.
//Both report on the same sets.
internal static class PublicBiddingSets
{
    public static async Task<IReadOnlyList<LeagueYearPublicBiddingSet>> GetForActiveYears(InterLeagueService interLeagueService, GameAcquisitionService gameAcquisitionService)
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
