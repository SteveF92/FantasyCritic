namespace FantasyCritic.Lib.Interfaces;
public interface IDiscordRepo
{
    Task<IReadOnlyList<MinimalLeagueChannel>> GetAllLeagueChannels();
    Task SetLeagueChannel(Guid leagueID, ulong guildID, ulong channelID, int year);
    Task DeleteLeagueChannel(ulong guildID, ulong channelID);
    Task<LeagueChannel?> GetLeagueChannel(ulong guildID, ulong channelID, int year);
    Task SetIsGameNewsEnabled(Guid leagueID, ulong guildID, ulong channelID, bool isGameNewsEnabled);
}
