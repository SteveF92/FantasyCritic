namespace FantasyCritic.Lib.Interfaces;
public interface IDiscordSupplementalDataRepo
{
    Task<IReadOnlySet<Guid>> GetLeaguesWithOrFormerlyWithGame(MasterGameYear masterGameYear);
    Task<ILookup<Guid, Guid>> GetLeaguesWithOrFormerlyWithGames(IEnumerable<MasterGameYear> masterGamesReleasingToday, int year);
}
