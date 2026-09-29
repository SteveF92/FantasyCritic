namespace FantasyCritic.Lib.Discord.Models;

//Each message carries the ID of its row in the pending table. A new message gets a fresh one; the spoof endpoints' messages are never stored.
public record NewMasterGameMessage(Guid PendingUpdateID, MasterGame MasterGame)
{
    public NewMasterGameMessage(MasterGame masterGame)
        : this(Guid.NewGuid(), masterGame)
    {
    }
}

public record GameCriticScoreUpdateMessage(Guid PendingUpdateID, MasterGame Game, decimal? OldCriticScore, decimal? NewCriticScore)
{
    public GameCriticScoreUpdateMessage(MasterGame game, decimal? oldCriticScore, decimal? newCriticScore)
        : this(Guid.NewGuid(), game, oldCriticScore, newCriticScore)
    {
    }
}

//Year is the year both games were read in, which is what the release statuses are compared for.
public record MasterGameEditMessage(Guid PendingUpdateID, MasterGame ExistingGame, MasterGame EditedGame, int Year, IReadOnlyList<string> Changes)
{
    public MasterGameEditMessage(MasterGame existingGame, MasterGame editedGame, int year, IReadOnlyList<string> changes)
        : this(Guid.NewGuid(), existingGame, editedGame, year, changes)
    {
    }

    public WillReleaseStatus? PreviousReleaseStatus =>
        ExistingGame.GetWillReleaseStatus(Year) != EditedGame.GetWillReleaseStatus(Year) ? ExistingGame.GetWillReleaseStatus(Year) : null;
}

public record PendingMasterGameUpdates(IReadOnlyList<NewMasterGameMessage> NewGames, IReadOnlyList<GameCriticScoreUpdateMessage> ScoreUpdates,
    IReadOnlyList<MasterGameEditMessage> Edits)
{
    public IReadOnlyList<Guid> PendingUpdateIDs => NewGames.Select(x => x.PendingUpdateID)
        .Concat(ScoreUpdates.Select(x => x.PendingUpdateID))
        .Concat(Edits.Select(x => x.PendingUpdateID))
        .ToList();
}

//What SendPendingMasterGameUpdates did. BotAvailable is false when the bot is disabled or never became ready, and then nothing was read or sent.
public record MasterGameUpdatesSendResult(bool BotAvailable, int NewGames, int ScoreUpdates, int Edits)
{
    public static readonly MasterGameUpdatesSendResult BotUnavailable = new MasterGameUpdatesSendResult(false, 0, 0, 0);
}
