using Discord;
using FantasyCritic.Lib.Discord.Interfaces;
using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Lib.Discord.Utilities;
public class DiscordFormatter : IDiscordFormatter
{
    private readonly DiscordSettings _discordSettings;

    public DiscordFormatter(DiscordSettings discordSettings)
    {
        _discordSettings = discordSettings;
    }

    public Embed BuildRegularEmbed(string title, string messageText, IUser? user = null, IList<EmbedFieldBuilder>? embedFieldBuilders = null, string url = "")
    {
        return BuildEmbed(title, messageText, user, _discordSettings.EmbedColors.Regular, embedFieldBuilders, url).Build();
    }
    public Embed BuildErrorEmbed(string title, string messageText, IUser? user = null, IList<EmbedFieldBuilder>? embedFieldBuilders = null, string url = "")
    {
        return BuildEmbed(title, messageText, user, _discordSettings.EmbedColors.Error, embedFieldBuilders, url).Build();
    }

    private EmbedBuilder BuildEmbed(string title,
        string messageText,
        IUser? user = null,
        uint embedColor = 0,
        IList<EmbedFieldBuilder>? embedFieldBuilders = null,
        string url = "")
    {
        var embedBuilder = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(messageText)
            .WithColor(embedColor == 0 ? _discordSettings.EmbedColors.Regular : embedColor)
            .WithCurrentTimestamp();

        if (user != null)
        {
            embedBuilder.WithFooter(BuildEmbedFooter(user));
        }
        if (embedFieldBuilders != null && embedFieldBuilders.Any())
        {
            embedBuilder.WithFields(embedFieldBuilders);
        }
        if (!string.IsNullOrEmpty(url))
        {
            embedBuilder.WithUrl(url);
        }

        return embedBuilder;
    }

    private EmbedFooterBuilder BuildEmbedFooter(IUser user)
    {
        return new EmbedFooterBuilder()
            .WithText(GetEmbedFooterText(user))
            .WithIconUrl(GetUserAvatar(user));
    }

    private string GetEmbedFooterText(IUser user)
    {
        return $"Requested by {user.Username}";
    }

    private string GetUserAvatar(IUser user)
    {
        return user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
    }

}
