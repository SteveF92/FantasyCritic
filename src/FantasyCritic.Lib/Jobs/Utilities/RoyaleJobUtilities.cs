using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Utilities;

internal static class RoyaleJobUtilities
{
    public static async Task RecalculateRoyaleWinners(RoyaleService royaleService)
    {
        var supportedQuarters = await royaleService.GetYearQuarters();
        foreach (var supportedQuarter in supportedQuarters)
        {
            bool readyToCalculateWinners = supportedQuarter.Finished && supportedQuarter.WinningUser is null;
            if (!readyToCalculateWinners)
            {
                continue;
            }

            await royaleService.CalculateRoyaleWinnerForQuarter(supportedQuarter);
        }
    }
}
