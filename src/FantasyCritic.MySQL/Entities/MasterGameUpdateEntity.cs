using System.Text.Json;
using FantasyCritic.Lib;
using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.MySQL.Entities;

internal class MasterGameUpdateType : TypeSafeEnum<MasterGameUpdateType>
{
    public static readonly MasterGameUpdateType NewGame = new MasterGameUpdateType("NewGame");
    public static readonly MasterGameUpdateType ScoreUpdate = new MasterGameUpdateType("ScoreUpdate");
    public static readonly MasterGameUpdateType Edit = new MasterGameUpdateType("Edit");

    private MasterGameUpdateType(string value)
        : base(value)
    {
    }

    public override string ToString() => Value;
}

//A row of tbl_discord_mastergameupdate. MasterGameSnapshot is the game as created, before the score was saved, or before the edit.
//The other columns are filled for their own kind only.
internal class MasterGameUpdateEntity
{
    public MasterGameUpdateEntity()
    {

    }

    public MasterGameUpdateEntity(NewMasterGameMessage message, Instant queuedTimestamp)
        : this(message.MasterGameUpdateID, MasterGameUpdateType.NewGame, message.MasterGame, queuedTimestamp)
    {
    }

    public MasterGameUpdateEntity(GameCriticScoreUpdateMessage message, Instant queuedTimestamp)
        : this(message.MasterGameUpdateID, MasterGameUpdateType.ScoreUpdate, message.Game, queuedTimestamp)
    {
        OldCriticScore = message.OldCriticScore;
        NewCriticScore = message.NewCriticScore;
    }

    public MasterGameUpdateEntity(MasterGameEditMessage message, Instant queuedTimestamp)
        : this(message.MasterGameUpdateID, MasterGameUpdateType.Edit, message.ExistingGame, queuedTimestamp)
    {
        EditedMasterGameSnapshot = new MasterGameSnapshotEntity(message.EditedGame).ToJson();
        Year = message.Year;
        Changes = JsonSerializer.Serialize(message.Changes, FantasyCriticJsonOptions.Default);
    }

    private MasterGameUpdateEntity(Guid masterGameUpdateID, MasterGameUpdateType updateType, MasterGame masterGame, Instant queuedTimestamp)
    {
        MasterGameUpdateID = masterGameUpdateID;
        UpdateType = updateType.Value;
        MasterGameID = masterGame.MasterGameID;
        MasterGameSnapshot = new MasterGameSnapshotEntity(masterGame).ToJson();
        QueuedTimestamp = queuedTimestamp;
    }

    public Guid MasterGameUpdateID { get; set; }
    public string UpdateType { get; set; } = null!;
    public Guid MasterGameID { get; set; }
    public string MasterGameSnapshot { get; set; } = null!;
    public string? EditedMasterGameSnapshot { get; set; }
    public int? Year { get; set; }
    public decimal? OldCriticScore { get; set; }
    public decimal? NewCriticScore { get; set; }
    public string? Changes { get; set; }
    public Instant QueuedTimestamp { get; set; }

    public MasterGameUpdateType GetUpdateType() => MasterGameUpdateType.FromValue(UpdateType);

    public NewMasterGameMessage ToNewGameMessage(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        return new NewMasterGameMessage(MasterGameUpdateID, GetMasterGame(tagDictionary));
    }

    public GameCriticScoreUpdateMessage ToScoreUpdateMessage(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        return new GameCriticScoreUpdateMessage(MasterGameUpdateID, GetMasterGame(tagDictionary), OldCriticScore, NewCriticScore);
    }

    public MasterGameEditMessage ToEditMessage(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        if (EditedMasterGameSnapshot is null || !Year.HasValue || Changes is null)
        {
            throw new Exception($"Pending edit {MasterGameUpdateID} is missing its edited snapshot, year or changes.");
        }

        var editedGame = MasterGameSnapshotEntity.FromJson(EditedMasterGameSnapshot).ToDomain(tagDictionary);
        var changes = JsonSerializer.Deserialize<List<string>>(Changes, FantasyCriticJsonOptions.Default)
                      ?? throw new Exception($"Pending edit {MasterGameUpdateID} has null changes.");
        return new MasterGameEditMessage(MasterGameUpdateID, GetMasterGame(tagDictionary), editedGame, Year.Value, changes);
    }

    private MasterGame GetMasterGame(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        return MasterGameSnapshotEntity.FromJson(MasterGameSnapshot).ToDomain(tagDictionary);
    }
}
