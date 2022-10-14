using Discord.WebSocket;
using NodaTime;

namespace FantasyCritic.Lib.Discord.Interfaces;

public interface IDiscordParameterParser
{
    LocalDate? GetDateFromProvidedYear(SocketSlashCommandDataOption? providedYear);
    LocalDate? GetDateFromProvidedYear(int? providedYear = null);
}
