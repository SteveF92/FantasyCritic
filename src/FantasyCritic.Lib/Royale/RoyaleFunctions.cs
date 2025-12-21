namespace FantasyCritic.Lib.Royale;
public static class RoyaleFunctions
{
    public static bool GameOrActionIsHidden(RoyaleYearQuarter yearQuarter, MasterGameYear masterGame, LocalDate currentDate)
    {
        if (yearQuarter.Finished)
        {
            return false;
        }

        if (masterGame.MasterGame.CriticScore.HasValue)
        {
            return false;
        }

        if (masterGame.MasterGame.IsReleased(currentDate))
        {
            if (SupportedYear.Year2026FeatureSupported(yearQuarter.YearQuarter.Year))
            {
                if (!masterGame.MasterGame.CriticScore.HasValue && masterGame.MasterGame.ReleaseDate!.Value == currentDate)
                {
                    return false;
                }
            }

            return false;
        }

        if (!masterGame.CouldReleaseInQuarter(yearQuarter.YearQuarter))
        {
            return false;
        }

        return true;
    }
}
