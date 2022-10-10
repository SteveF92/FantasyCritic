using Discord;
using Discord.WebSocket;

namespace FantasyCritic.Discord.Interfaces;

public interface IDiscordFormatter
{
    public EmbedFooterBuilder BuildEmbedFooter(SocketUser user);
}
