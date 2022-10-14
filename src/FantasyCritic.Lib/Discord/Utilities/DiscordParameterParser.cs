using Discord.WebSocket;
using FantasyCritic.Lib.Discord.Interfaces;
using NodaTime;

namespace FantasyCritic.Lib.Discord.Utilities;
public class DiscordParameterParser : IDiscordParameterParser
{
    public LocalDate? GetDateFromProvidedYear(SocketSlashCommandDataOption? providedYear)
    {
        if (providedYear != null)
        {
            var yearValue = (long)providedYear.Value;
            var convertedYear = Convert.ToInt32(yearValue);
            return new LocalDate(convertedYear, 12, 31);
        }
        return null;
    }

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
