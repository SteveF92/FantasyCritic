using Discord;

namespace FantasyCritic.Lib.Discord.Interfaces;

public interface IDiscordFormatter
{
    Embed BuildRegularEmbed(string title, string messageText, IUser user, IList<EmbedFieldBuilder>? embedFieldBuilders = null, string url = "");
    Embed BuildErrorEmbed(string title, string messageText, IUser user, IList<EmbedFieldBuilder>? embedFieldBuilders = null, string url = "");
    EmbedFooterBuilder BuildEmbedFooter(IUser user);
}
