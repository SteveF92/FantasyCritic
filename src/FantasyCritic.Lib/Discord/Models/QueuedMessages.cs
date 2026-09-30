namespace FantasyCritic.Lib.Discord.Models;

public record NewMasterGameMessage(Guid PendingUpdateID, MasterGame MasterGame);

public record GameCriticScoreUpdateMessage(Guid PendingUpdateID, MasterGame Game, decimal? OldCriticScore, decimal? NewCriticScore);

//Year is the year both games were read in, which is what the release statuses are compared for.
public record MasterGameEditMessage(Guid PendingUpdateID, MasterGame ExistingGame, MasterGame EditedGame, int Year, IReadOnlyList<string> Changes)
{
    public WillReleaseStatus? PreviousReleaseStatus =>
        !ExistingGame.GetWillReleaseStatus(Year).Equals(EditedGame.GetWillReleaseStatus(Year)) ? ExistingGame.GetWillReleaseStatus(Year) : null;
}

public record PendingMasterGameUpdates(IReadOnlyList<NewMasterGameMessage> NewGames, IReadOnlyList<GameCriticScoreUpdateMessage> ScoreUpdates, IReadOnlyList<MasterGameEditMessage> Edits)
{
    public IReadOnlyList<Guid> PendingUpdateIDs => NewGames.Select(x => x.PendingUpdateID)
        .Concat(ScoreUpdates.Select(x => x.PendingUpdateID))
        .Concat(Edits.Select(x => x.PendingUpdateID))
        .ToList();
}

public record MasterGameUpdatesSendResult(bool BotAvailable, int NewGames, int ScoreUpdates, int Edits)
{
    public static readonly MasterGameUpdatesSendResult BotUnavailable = new MasterGameUpdatesSendResult(false, 0, 0, 0);
}
