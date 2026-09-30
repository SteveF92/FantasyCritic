namespace FantasyCritic.Lib.Discord.Models;

public record NewMasterGameMessage(Guid MasterGameUpdateID, MasterGame MasterGame);

public record GameCriticScoreUpdateMessage(Guid MasterGameUpdateID, MasterGame Game, decimal? OldCriticScore, decimal? NewCriticScore);

//Year is the year both games were read in, which is what the release statuses are compared for.
public record MasterGameEditMessage(Guid MasterGameUpdateID, MasterGame ExistingGame, MasterGame EditedGame, int Year, IReadOnlyList<string> Changes)
{
    public WillReleaseStatus? PreviousReleaseStatus =>
        !ExistingGame.GetWillReleaseStatus(Year).Equals(EditedGame.GetWillReleaseStatus(Year)) ? ExistingGame.GetWillReleaseStatus(Year) : null;
}

public record PendingMasterGameUpdates(IReadOnlyList<NewMasterGameMessage> NewGames, IReadOnlyList<GameCriticScoreUpdateMessage> ScoreUpdates, IReadOnlyList<MasterGameEditMessage> Edits)
{
    public IReadOnlyList<Guid> MasterGameUpdateIDs => NewGames.Select(x => x.MasterGameUpdateID)
        .Concat(ScoreUpdates.Select(x => x.MasterGameUpdateID))
        .Concat(Edits.Select(x => x.MasterGameUpdateID))
        .ToList();
}

public record MasterGameUpdatesSendResult(bool BotAvailable, int NewGames, int ScoreUpdates, int Edits)
{
    public static readonly MasterGameUpdatesSendResult BotUnavailable = new MasterGameUpdatesSendResult(false, 0, 0, 0);
}
