namespace FantasyCritic.Lib.Discord;
public interface IDiscordChannel
{
    ulong GuildID { get; }
    ulong ChannelID { get; }
}
