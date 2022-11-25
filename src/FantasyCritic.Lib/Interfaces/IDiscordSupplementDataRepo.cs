namespace FantasyCritic.Lib.Interfaces;
public interface IDiscordSupplementalDataRepo
{
    Task<IReadOnlySet<Guid>> GetLeaguesWithOrFormerlyWithGame(MasterGameYear masterGameYear);
}
