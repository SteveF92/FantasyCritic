using Discord.WebSocket;
using NodaTime;

namespace FantasyCritic.Discord;
public class ParameterParser : IParameterParser
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
}
