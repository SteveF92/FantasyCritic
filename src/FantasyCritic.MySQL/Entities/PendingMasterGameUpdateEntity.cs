using System.Text.Json;
using FantasyCritic.Lib;
using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.MySQL.Entities;

internal class PendingMasterGameUpdateType : TypeSafeEnum<PendingMasterGameUpdateType>
{
    public static readonly PendingMasterGameUpdateType NewGame = new PendingMasterGameUpdateType("NewGame");
    public static readonly PendingMasterGameUpdateType ScoreUpdate = new PendingMasterGameUpdateType("ScoreUpdate");
    public static readonly PendingMasterGameUpdateType Edit = new PendingMasterGameUpdateType("Edit");

    private PendingMasterGameUpdateType(string value)
        : base(value)
    {
    }

    public override string ToString() => Value;
}

//A row of tbl_discord_pendingmastergameupdate. MasterGameSnapshot is the game as created, before the score was saved, or before the edit.
//The other columns are filled for their own kind only.
internal class PendingMasterGameUpdateEntity
{
    public PendingMasterGameUpdateEntity()
    {

    }

    public PendingMasterGameUpdateEntity(NewMasterGameMessage message, Instant queuedTimestamp)
        : this(PendingMasterGameUpdateType.NewGame, message.MasterGame, queuedTimestamp)
    {
    }

    public PendingMasterGameUpdateEntity(GameCriticScoreUpdateMessage message, Instant queuedTimestamp)
        : this(PendingMasterGameUpdateType.ScoreUpdate, message.Game, queuedTimestamp)
    {
        OldCriticScore = message.OldCriticScore;
        NewCriticScore = message.NewCriticScore;
    }

    public PendingMasterGameUpdateEntity(MasterGameEditMessage message, Instant queuedTimestamp)
        : this(PendingMasterGameUpdateType.Edit, message.ExistingGame, queuedTimestamp)
    {
        EditedMasterGameSnapshot = new MasterGameSnapshotEntity(message.EditedGame).ToJson();
        Year = message.Year;
        Changes = JsonSerializer.Serialize(message.Changes, FantasyCriticJsonOptions.Default);
    }

    private PendingMasterGameUpdateEntity(PendingMasterGameUpdateType updateType, MasterGame masterGame, Instant queuedTimestamp)
    {
        PendingUpdateID = Guid.NewGuid();
        UpdateType = updateType.Value;
        MasterGameID = masterGame.MasterGameID;
        MasterGameSnapshot = new MasterGameSnapshotEntity(masterGame).ToJson();
        QueuedTimestamp = queuedTimestamp;
    }

    public Guid PendingUpdateID { get; set; }
    public string UpdateType { get; set; } = null!;
    public Guid MasterGameID { get; set; }
    public string MasterGameSnapshot { get; set; } = null!;
    public string? EditedMasterGameSnapshot { get; set; }
    public int? Year { get; set; }
    public decimal? OldCriticScore { get; set; }
    public decimal? NewCriticScore { get; set; }
    public string? Changes { get; set; }
    public Instant QueuedTimestamp { get; set; }

    public PendingMasterGameUpdateType GetUpdateType() => PendingMasterGameUpdateType.FromValue(UpdateType);

    public NewMasterGameMessage ToNewGameMessage(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        return new NewMasterGameMessage(GetMasterGame(tagDictionary));
    }

    public GameCriticScoreUpdateMessage ToScoreUpdateMessage(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        return new GameCriticScoreUpdateMessage(GetMasterGame(tagDictionary), OldCriticScore, NewCriticScore);
    }

    public MasterGameEditMessage ToEditMessage(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        if (EditedMasterGameSnapshot is null || !Year.HasValue || Changes is null)
        {
            throw new Exception($"Pending edit {PendingUpdateID} is missing its edited snapshot, year or changes.");
        }

        var editedGame = MasterGameSnapshotEntity.FromJson(EditedMasterGameSnapshot).ToDomain(tagDictionary);
        var changes = JsonSerializer.Deserialize<List<string>>(Changes, FantasyCriticJsonOptions.Default)
                      ?? throw new Exception($"Pending edit {PendingUpdateID} has null changes.");
        return new MasterGameEditMessage(GetMasterGame(tagDictionary), editedGame, Year.Value, changes);
    }

    private MasterGame GetMasterGame(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        return MasterGameSnapshotEntity.FromJson(MasterGameSnapshot).ToDomain(tagDictionary);
    }
}
