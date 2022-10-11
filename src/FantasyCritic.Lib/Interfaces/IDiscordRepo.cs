namespace FantasyCritic.Lib.Interfaces;
public interface IDiscordRepo
{
    Task SetLeagueChannel(Guid leagueId, string channelId, int year);
    Task DeleteLeagueChannel(Guid leagueId, string channelId);
    Task<LeagueChannel?> GetLeagueChannel(string channelID, int year);
}
