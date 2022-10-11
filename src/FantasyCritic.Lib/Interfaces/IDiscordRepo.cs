namespace FantasyCritic.Lib.Interfaces;
public interface IDiscordRepo
{
    Task SetLeagueChannel(Guid leagueId, string channelId, int year);
    Task DeleteLeagueChannel(string channelId);
    Task<LeagueChannel?> GetLeagueChannel(string channelID, int year);
}
