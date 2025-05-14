using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Lib.Discord.Interfaces;
public interface IRelevantGameNewsHandler
{
    public bool IsNewGameNewsRelevant(NewGameNewsRecord newsRecord);
    public bool IsEditedGameNewsRelevant(EditedGameNewsRecord newsRecord);
    public bool IsReleasedGameNewsRelevant(ReleaseGameNewsRecord newsRecord);
    public bool IsScoreGameNewsRelevant(ScoreGameNewsRecord newsRecord);
}
