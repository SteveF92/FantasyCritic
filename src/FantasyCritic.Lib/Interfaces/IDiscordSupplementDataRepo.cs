namespace FantasyCritic.Lib.Interfaces;
public interface IDiscordSupplementalDataRepo
{
    Task<IReadOnlySet<Guid>> GetLeaguesWithOrFormerlyWithGame(MasterGame masterGame, int year);
    Task<ILookup<Guid, Guid>> GetLeaguesWithOrFormerlyWithGames(IEnumerable<Guid> masterGameIDs, int year);
    Task<ILookup<Guid, Guid>> GetLeaguesWithOrFormerlyWithGamesInUnfinishedYears(IEnumerable<Guid> masterGameIDs);
}
