namespace FantasyCritic.Lib.Interfaces;
public interface IDiscordRepo
{
    Task SetLeagueChannel(Guid leagueID, ulong guildID, ulong channelID, int year);
    Task SetIsGameNewsEnabled(Guid leagueID, ulong guildID, ulong channelID, bool isGameNewsEnabled);
    Task<bool> DeleteLeagueChannel(ulong guildID, ulong channelID);
    Task<IReadOnlyList<MinimalLeagueChannel>> GetAllLeagueChannels();
    Task<IReadOnlyList<MinimalLeagueChannel>?> GetLeagueChannels(Guid leagueID);
    Task<LeagueChannel?> GetLeagueChannel(ulong guildID, ulong channelID, int year);
}
