namespace FantasyCritic.Lib.Interfaces;
public interface IDiscordRepo
{
    Task<LeagueChannel?> GetLeagueChannel(string channelID, int year);
}
