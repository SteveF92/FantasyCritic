namespace FantasyCritic.Lib.Interfaces;
public interface IDiscordSupplementalDataRepo
{
    Task<bool> GameInLeagueOrFormerlyInLeague(MasterGameYear masterGameYear, Guid leagueID);
}
