namespace FantasyCritic.Lib.Interfaces;
public interface IDiscordRepo
{
    Task SetLeagueChannel(Guid leagueID, ulong guildID, ulong channelID, int year);
    Task SetIsGameNewsSetting(Guid leagueID, ulong guildID, ulong channelID, DiscordGameNewsSetting gameNewsSetting);
    Task<bool> DeleteLeagueChannel(ulong guildID, ulong channelID);
    Task<IReadOnlyList<MinimalLeagueChannel>> GetAllLeagueChannels();
    Task<IReadOnlyList<MinimalLeagueChannel>> GetLeagueChannels(Guid leagueID);
    Task<MinimalLeagueChannel?> GetMinimalLeagueChannel(ulong guildID, ulong channelID);
    Task<LeagueChannel?> GetLeagueChannel(ulong guildID, ulong channelID, IReadOnlyList<SupportedYear> supportedYears);
    Task<LeagueChannel?> GetLeagueChannel(ulong guildID, ulong channelID, int year);
    Task RemoveAllLeagueChannelsForLeague(Guid leagueID);
}
