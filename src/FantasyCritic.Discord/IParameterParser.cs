using Discord.WebSocket;
using NodaTime;

namespace FantasyCritic.Discord;

public interface IParameterParser
{
    LocalDate? GetDateFromProvidedYear(SocketSlashCommandDataOption? providedYear);
}
