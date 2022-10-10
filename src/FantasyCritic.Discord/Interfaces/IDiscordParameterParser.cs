using Discord.WebSocket;
using NodaTime;

namespace FantasyCritic.Discord.Interfaces;

public interface IDiscordParameterParser
{
    LocalDate? GetDateFromProvidedYear(SocketSlashCommandDataOption? providedYear);
}
