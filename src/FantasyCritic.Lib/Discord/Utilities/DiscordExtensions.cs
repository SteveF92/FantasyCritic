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
            Serilog.Log.Warning(e, "Error sending message to channel {ChannelID} {ChannelName}", channel.Id, channel.Name);
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
        TimeSpan[] delays = [TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(400), TimeSpan.FromMilliseconds(800)];
        for (int attempt = 1; attempt <=5; attempt++)
        {
            try
            {
                Serilog.Log.Information("Sending message to channel {ChannelID} {ChannelName} Attempt:{Attempt}", channel.Id, channel.Name, attempt);
                return await channel.SendMessageAsync(text, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags);
            }
            catch (Exception e)
            {
                Serilog.Log.Warning(e, "Error sending message to channel {ChannelID} {ChannelName}", channel.Id, channel.Name);
                if (e.Message.Contains("503: ServiceUnavailable"))
                {
                    await Task.Delay(delays[attempt - 1]);
                    continue;
                }
                else
                {
                    return null;
                }
            }
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
