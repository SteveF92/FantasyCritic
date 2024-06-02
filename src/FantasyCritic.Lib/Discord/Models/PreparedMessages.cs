using Discord;
using Discord.WebSocket;

namespace FantasyCritic.Lib.Discord.Models;

public record PreparedDiscordMessage(SocketTextChannel Channel, string? Message = null, Embed? Embed = null);
