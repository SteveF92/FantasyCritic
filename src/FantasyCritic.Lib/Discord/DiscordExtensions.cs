using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace FantasyCritic.Lib.Discord;
public static class DiscordExtensions
{
    public static Task<RestUserMessage?> TrySendMessageAsync(this SocketTextChannel channel, string messageToSend)
    {
        try
        {
            return channel.SendMessageAsync(messageToSend);
        }
        catch (Exception e)
        {
            Serilog.Log.Error(e, "Error sending message to channel {ChannelID} {ChannelName}", channel.Id, channel.Name);
        }

        return Task.FromResult<RestUserMessage?>(null);
    }

    public static Task<RestUserMessage?> TrySendMessageAsync(
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
            return channel.SendMessageAsync(text, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags);
        }
        catch (Exception e)
        {
            Serilog.Log.Error(e, "Error sending message to channel {ChannelID} {ChannelName}", channel.Id, channel.Name);
        }

        return Task.FromResult<RestUserMessage?>(null);

    }
}
