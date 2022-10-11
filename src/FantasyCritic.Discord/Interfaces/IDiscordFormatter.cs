using Discord;
using Discord.WebSocket;

namespace FantasyCritic.Discord.Interfaces;

public interface IDiscordFormatter
{
    Embed BuildRegularEmbed(string title, string messageText, SocketUser user, string url = "");
    Embed BuildErrorEmbed(string title, string text, SocketUser user);
    EmbedFooterBuilder BuildEmbedFooter(SocketUser user);
}
