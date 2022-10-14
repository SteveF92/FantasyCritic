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

    public Embed BuildRegularEmbed(string title, string messageText, IUser user, IList<EmbedFieldBuilder>? embedFieldBuilders = null, string url = "")
    {
        return BuildEmbed(title, messageText, user, _discordSettings.EmbedColors.Regular, embedFieldBuilders, url).Build();
    }
    public Embed BuildErrorEmbed(string title, string messageText, IUser user, IList<EmbedFieldBuilder>? embedFieldBuilders = null, string url = "")
    {
        return BuildEmbed(title, messageText, user, _discordSettings.EmbedColors.Error, embedFieldBuilders, url).Build();
    }

    private EmbedBuilder BuildEmbed(string title,
        string messageText,
        IUser user,
        uint embedColor,
        IList<EmbedFieldBuilder>? embedFieldBuilders,
        string url = "")
    {
        var embedBuilder = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(messageText)
            .WithFooter(BuildEmbedFooter(user))
            .WithColor(embedColor)
            .WithCurrentTimestamp();

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

    public EmbedFooterBuilder BuildEmbedFooter(IUser user)
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
