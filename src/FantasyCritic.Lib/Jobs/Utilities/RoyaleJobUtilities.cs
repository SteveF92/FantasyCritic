using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Jobs.Utilities;

internal static class RoyaleJobUtilities
{
    public static async Task RecalculateRoyaleWinners(IRoyaleRepo royaleRepo)
    {
        var supportedQuarters = await royaleRepo.GetYearQuarters();
        foreach (var supportedQuarter in supportedQuarters)
        {
            bool readyToCalculateWinners = supportedQuarter.Finished && supportedQuarter.WinningUser is null;
            if (!readyToCalculateWinners)
            {
                continue;
            }

            await royaleRepo.CalculateRoyaleWinnerForQuarter(supportedQuarter.YearQuarter.Year, supportedQuarter.YearQuarter.Quarter);
        }
    }
}
