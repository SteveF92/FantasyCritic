using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Royale;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Utilities;

internal static class RoyaleJobUtilities
{
    //Returns the quarters it calculated, reloaded so the caller can report who won. A quarter can still come back without a winner,
    //if none of its publishers has a game.
    public static async Task<IReadOnlyList<RoyaleYearQuarter>> CalculateMissingWinners(IRoyaleRepo royaleRepo, ILogger logger, CancellationToken cancellationToken)
    {
        var supportedQuarters = await royaleRepo.GetYearQuarters();
        var quartersMissingWinners = supportedQuarters.Where(x => x.Finished && x.WinningUser is null).Select(x => x.YearQuarter).ToHashSet();
        if (quartersMissingWinners.Count == 0)
        {
            logger.LogDebug("No finished Royale quarters are missing a winner.");
            return [];
        }

        foreach (var yearQuarter in quartersMissingWinners.Order())
        {
            cancellationToken.ThrowIfCancellationRequested();
            await royaleRepo.CalculateRoyaleWinnerForQuarter(yearQuarter.Year, yearQuarter.Quarter);
        }

        var reloadedQuarters = await royaleRepo.GetYearQuarters();
        var calculatedQuarters = reloadedQuarters.Where(x => quartersMissingWinners.Contains(x.YearQuarter)).ToList();
        foreach (var quarter in calculatedQuarters)
        {
            if (quarter.WinningUser is null)
            {
                logger.LogWarning("Royale quarter {YearQuarter} is finished but has no winner: none of its publishers has a game.", quarter.YearQuarter);
            }
            else
            {
                logger.LogInformation("Royale quarter {YearQuarter} was won by {WinnerDisplayName} ({WinnerUserID}).",
                    quarter.YearQuarter, quarter.WinningUser.DisplayName, quarter.WinningUser.UserID);
            }
        }

        return calculatedQuarters;
    }

    public static string DescribeWinners(IReadOnlyList<RoyaleYearQuarter> calculatedQuarters)
    {
        if (calculatedQuarters.Count == 0)
        {
            return "No winners to calculate.";
        }

        var descriptions = calculatedQuarters.Select(x => x.WinningUser is null
            ? $"{x.YearQuarter} has no winner."
            : $"{x.YearQuarter} was won by {x.WinningUser.DisplayName}.");
        return string.Join(" ", descriptions);
    }
}
