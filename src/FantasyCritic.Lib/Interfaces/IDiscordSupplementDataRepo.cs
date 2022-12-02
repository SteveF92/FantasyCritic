namespace FantasyCritic.Lib.Interfaces;
public interface IDiscordSupplementalDataRepo
{
    Task<IReadOnlySet<Guid>> GetLeaguesWithOrFormerlyWithGame(MasterGame masterGame, int year);
    Task<ILookup<Guid, Guid>> GetLeaguesWithOrFormerlyWithGames(IEnumerable<MasterGame> masterGames, int year);
}
