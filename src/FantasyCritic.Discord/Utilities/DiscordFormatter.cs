using Discord;
using Discord.WebSocket;
using FantasyCritic.Discord.Interfaces;

namespace FantasyCritic.Discord.Utilities;
public class DiscordFormatter : IDiscordFormatter
{
    public EmbedFooterBuilder BuildEmbedFooter(SocketUser user)
    {
        var embedFooterBuilder = new EmbedFooterBuilder()
            .WithText(GetEmbedFooterText(user))
            .WithIconUrl(GetUserAvatar(user));
        return embedFooterBuilder;
    }

    private string GetEmbedFooterText(SocketUser user)
    {
        return $"Requested by {user.Username}";
    }

    private string GetUserAvatar(SocketUser user)
    {
        return user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
    }

}
