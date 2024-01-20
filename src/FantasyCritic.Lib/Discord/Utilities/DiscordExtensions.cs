using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace FantasyCritic.Lib.Discord.Utilities;
public static class DiscordExtensions
{
    public static async Task<RestUserMessage?> TrySendMessageAsync(this SocketTextChannel channel, string messageToSend)
    {
        try
        {
            return await channel.SendMessageAsync(messageToSend);
        }
        catch (Exception e)
        {
            Serilog.Log.Error(e, "Error sending message to channel {ChannelID} {ChannelName}", channel.Id, channel.Name);
        }

        return null;
    }

    public static async Task<RestUserMessage?> TrySendMessageAsync(
        this SocketTextChannel channel,
        string? text = null,
        bool isTTS = false,
        Embed? embed = null,
        RequestOptions? options = null,
        AllowedMentions? allowedMentions = null,
        MessageReference? messageReference = null,
        MessageComponent? components = null,
        ISticker[]? stickers = null,
        Embed[]? embeds = null,
        MessageFlags flags = MessageFlags.None)
    {
        try
        {
            return await channel.SendMessageAsync(text, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags);
        }
        catch (Exception e)
        {
            Serilog.Log.Error(e, "Error sending message to channel {ChannelID} {ChannelName}", channel.Id, channel.Name);
        }

        return null;

    }

    public static bool HasPermissionToSendMessagesInChannel(this ISocketMessageChannel channel, ulong userID)
    {
        if (channel is not IGuildChannel guildChannel)
        {
            return false;
        }
        if (guildChannel.Guild is not SocketGuild guild)
        {
            return false;
        }

        var botUserInGuild = guild.Users.FirstOrDefault(u => u.Id == userID);
        if (botUserInGuild == null)
        {
            return false;
        }
        var permissions = botUserInGuild.GetPermissions(guildChannel);
        return permissions.SendMessages;
    }
}
