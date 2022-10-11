using Discord;
using Discord.WebSocket;
using FantasyCritic.Discord.Interfaces;
using FantasyCritic.Discord.Models;

namespace FantasyCritic.Discord.Utilities;
public class DiscordFormatter : IDiscordFormatter
{
    private readonly DiscordSettings _discordSettings;

    public DiscordFormatter(DiscordSettings discordSettings)
    {
        _discordSettings = discordSettings;
    }

    public Embed BuildRegularEmbed(string title, string messageText, SocketUser user, string url = "")
    {
        var embedBuilder = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(messageText)
            .WithFooter(BuildEmbedFooter(user))
            .WithColor(_discordSettings.EmbedColors.Regular)
            .WithCurrentTimestamp();
        if (!string.IsNullOrEmpty(url))
        {
            embedBuilder.WithUrl(url);
        }

        return embedBuilder.Build();
    }

    public Embed BuildErrorEmbed(string title, string text, SocketUser user)
    {
        var embedBuilder = new EmbedBuilder()
            .WithDescription(text)
            .WithTitle(title)
            .WithFooter(BuildEmbedFooter(user))
            .WithColor(_discordSettings.EmbedColors.Error)
            .WithCurrentTimestamp();
        return embedBuilder.Build();
    }

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
