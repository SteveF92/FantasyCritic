using System.Globalization;

namespace FantasyCritic.Lib.Extensions;
public static class AnnualDateExtensions
{
    public static string ToReadableString(this AnnualDate date)
    {
        return date.ToString("MMMM d", CultureInfo.InvariantCulture);
    }

    public static string ToReadableString(this AnnualDate? date)
    {
        if (!date.HasValue)
        {
            return "No Value";
        }

        return date.Value.ToString("MMMM d", CultureInfo.InvariantCulture);
    }
}
