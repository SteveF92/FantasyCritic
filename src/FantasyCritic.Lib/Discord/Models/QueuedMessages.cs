namespace FantasyCritic.Lib.Discord.Models;

public record NewMasterGameMessage(MasterGame MasterGame);
public record GameCriticScoreUpdateMessage(MasterGame Game, decimal? OldCriticScore, decimal? NewCriticScore);

//Year is the year both games were read in, which is what the release statuses are compared for.
public record MasterGameEditMessage(MasterGame ExistingGame, MasterGame EditedGame, int Year, IReadOnlyList<string> Changes)
{
    public WillReleaseStatus? PreviousReleaseStatus =>
        ExistingGame.GetWillReleaseStatus(Year) != EditedGame.GetWillReleaseStatus(Year) ? ExistingGame.GetWillReleaseStatus(Year) : null;
}

//PendingUpdateIDs are the rows these came from, so a send deletes only what it read.
public record PendingMasterGameUpdates(IReadOnlyList<Guid> PendingUpdateIDs, IReadOnlyList<NewMasterGameMessage> NewGames,
    IReadOnlyList<GameCriticScoreUpdateMessage> ScoreUpdates, IReadOnlyList<MasterGameEditMessage> Edits);
