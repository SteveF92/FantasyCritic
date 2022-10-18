using FantasyCritic.Lib.Discord.Interfaces;

namespace FantasyCritic.Lib.Discord.Utilities;
public class DiscordParameterParser : IDiscordParameterParser
{
    public LocalDate? GetDateFromProvidedYear(int? providedYear = null)
    {
        if (providedYear != null)
        {
            var yearValue = (long)providedYear;
            var convertedYear = Convert.ToInt32(yearValue);
            return new LocalDate(convertedYear, 12, 31);
        }
        return null;
    }
}
