using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Lib.Interfaces;
public interface IDiscordRepo
{
    Task SetLeagueChannel(Guid leagueID, ulong guildID, ulong channelID);
    Task SetConferenceChannel(Guid conferenceID, ulong guildID, ulong channelID);
    Task SetLeagueGameNewsSetting(Guid leagueID, ulong guildID, ulong channelID, bool showPickedGameNews, bool showEligibleGameNews, bool showIneligibleGameNews, NotableMissSetting notableMissSetting);
    Task SetGameNewsSetting(ulong guildID, ulong channelID, GameNewsSetting gameNewsSetting);
    Task SetSkippedGameNewsTags(ulong guildID, ulong channelID, IEnumerable<MasterGameTag> skippedTags);
    Task SetBidAlertRoleId(Guid leagueID, ulong guildID, ulong channelID, ulong? bidAlertRoleID);
    Task<bool> DeleteLeagueChannel(ulong guildID, ulong channelID);
    Task<bool> DeleteConferenceChannel(ulong guildID, ulong channelID);
    Task<IReadOnlyList<MinimalLeagueChannel>> GetAllLeagueChannels();
    Task<IReadOnlyList<GameNewsChannel>> GetAllGameNewsChannels();
    Task<IReadOnlyList<MinimalLeagueChannel>> GetLeagueChannels(Guid leagueID);
    Task<IReadOnlyList<MinimalConferenceChannel>> GetConferenceChannels(Guid conferenceID);
    Task<MinimalLeagueChannel?> GetMinimalLeagueChannel(ulong guildID, ulong channelID);
    Task<LeagueChannel?> GetLeagueChannel(ulong guildID, ulong channelID, IReadOnlyList<SupportedYear> supportedYears, int? year = null);
    Task<ConferenceChannel?> GetConferenceChannel(ulong guildID, ulong channelID, IReadOnlyList<SupportedYear> supportedYears, int? year = null);
    Task<GameNewsChannel?> GetGameNewsChannel(ulong guildID, ulong channelID);
    Task RemoveAllLeagueChannelsForLeague(Guid leagueID);
}
