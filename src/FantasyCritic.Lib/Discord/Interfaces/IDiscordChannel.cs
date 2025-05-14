using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Lib.Discord.Interfaces;
public interface IDiscordChannel
{
    ulong GuildID { get; }
    ulong ChannelID { get; }
    DiscordChannelKey ChannelKey { get; }
}
