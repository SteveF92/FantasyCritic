using Discord;
using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Lib.Discord.Utilities;
public static class DiscordRateLimitUtilities
{
    public static readonly TimeSpan Delay = TimeSpan.FromSeconds(1);
    public const int MessagesPerSecond = 45;

    public static async Task RateLimitMessages(IEnumerable<PreparedDiscordMessage> messages, MessageFlags flags = MessageFlags.None)
    {
        var batches = messages.Chunk(MessagesPerSecond);
        foreach (var batch in batches)
        {
            var tasks = batch.Select(x => x.Channel.TrySendMessageAsync(x.Message, embed: x.Embed, flags: flags));
            await Task.WhenAll(tasks);
            await Task.Delay(Delay);
        }
    }
}
