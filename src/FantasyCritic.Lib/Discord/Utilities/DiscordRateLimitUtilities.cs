using Discord;
using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Lib.Discord.Utilities;

public static class DiscordRateLimitUtilities
{
    public static readonly TimeSpan Delay = TimeSpan.FromSeconds(1);
    public const int MessagesPerSecond = 45;

    public static async Task<int> RateLimitMessages(IEnumerable<PreparedDiscordMessage> messages, MessageFlags flags = MessageFlags.None)
    {
        var failedMessageCount = 0;
        var batches = messages.Chunk(MessagesPerSecond);
        foreach (var batch in batches)
        {
            var tasks = batch.Select(x => x.Channel.TrySendMessageAsync(x.Message, embed: x.Embed, flags: flags));
            var results = await Task.WhenAll(tasks);
            failedMessageCount += results.Count(x => x is null);
            await Task.Delay(Delay);
        }

        return failedMessageCount;
    }
}
