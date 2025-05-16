namespace FantasyCritic.Lib.Discord.Interfaces;
public interface IGameNewsRelevanceHandler
{
    public bool NewGameIsRelevant(MasterGame masterGame, LocalDate currentDate);
    public bool ExistingGameIsRelevant(MasterGame masterGame, bool releaseStatusChanged, LocalDate currentDate);
    public bool ReleasedGameIsRelevant(MasterGame masterGame);
    public bool ScoredGameIsRelevant(MasterGame masterGame, decimal? criticScore, LocalDate currentDate);
}
