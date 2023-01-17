using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Lib.Interfaces;
public interface IDiscordRepo
{
    Task SetLeagueChannel(Guid leagueID, ulong guildID, ulong channelID, int year);
    Task SetGameNewsSetting(Guid leagueID, ulong guildID, ulong channelID, bool sendLeagueMasterGameUpdates);
    Task SetGameNewsSetting(ulong guildID, ulong channelID, GameNewsSetting gameNewsSetting);
    Task SetBidAlertRoleId(Guid leagueID, ulong guildID, ulong channelID, ulong? bidAlertRoleID);
    Task<bool> DeleteLeagueChannel(ulong guildID, ulong channelID);
    Task<IReadOnlyList<MinimalLeagueChannel>> GetAllLeagueChannels();
    Task<IReadOnlyList<CombinedChannel>> GetAllCombinedChannels();
    Task<IReadOnlyList<MinimalLeagueChannel>> GetLeagueChannels(Guid leagueID);
    Task<MinimalLeagueChannel?> GetMinimalLeagueChannel(ulong guildID, ulong channelID);
    Task<LeagueChannel?> GetLeagueChannel(ulong guildID, ulong channelID, IReadOnlyList<SupportedYear> supportedYears, int? year = null);
    Task<LeagueChannel?> GetLeagueChannel(ulong guildID, ulong channelID, int year);
    Task<GameNewsChannel?> GetGameNewsChannel(ulong guildID, ulong channelID);
    Task RemoveAllLeagueChannelsForLeague(Guid leagueID);
}
