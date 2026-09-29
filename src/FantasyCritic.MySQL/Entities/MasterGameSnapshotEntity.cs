using System.Text.Json;
using FantasyCritic.Lib;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.MySQL.Entities;

//A MasterGame frozen as JSON, so a pending Discord update is judged on the game as it was when it was queued.
//It holds what MasterGame computes from, not what it computes: RawCriticScore rather than CriticScore, which averages the sub-games,
//and the sub-games themselves. MasterGameEntity can't do this, because it copies the computed CriticScore and skips OpenCriticSlug.
public class MasterGameSnapshotEntity
{
    public MasterGameSnapshotEntity()
    {

    }

    public MasterGameSnapshotEntity(MasterGame masterGame)
    {
        MasterGameID = masterGame.MasterGameID;
        GameName = masterGame.GameName;
        EstimatedReleaseDate = masterGame.EstimatedReleaseDate;
        MinimumReleaseDate = masterGame.MinimumReleaseDate;
        MaximumReleaseDate = masterGame.MaximumReleaseDate;
        EarlyAccessReleaseDate = masterGame.EarlyAccessReleaseDate;
        InternationalReleaseDate = masterGame.InternationalReleaseDate;
        AnnouncementDate = masterGame.AnnouncementDate;
        ReleaseDate = masterGame.ReleaseDate;
        OpenCriticID = masterGame.OpenCriticID;
        GGToken = masterGame.GGToken;
        GGSlug = masterGame.GGSlug;
        RawCriticScore = masterGame.RawCriticScore;
        HasAnyReviews = masterGame.HasAnyReviews;
        OpenCriticSlug = masterGame.OpenCriticSlug;
        Notes = masterGame.Notes;
        BoxartFileName = masterGame.BoxartFileName;
        GGCoverArtFileName = masterGame.GGCoverArtFileName;
        FirstCriticScoreTimestamp = masterGame.FirstCriticScoreTimestamp;
        SyncWithExternalAPIs = masterGame.SyncWithExternalAPIs;
        UseSimpleEligibility = masterGame.UseSimpleEligibility;
        DelayContention = masterGame.DelayContention;
        ShowNote = masterGame.ShowNote;
        AddedTimestamp = masterGame.AddedTimestamp;
        AddedByUserID = masterGame.AddedByUser.UserID;
        AddedByUserDisplayName = masterGame.AddedByUser.DisplayName;
        SubGames = masterGame.SubGames.Select(x => new MasterSubGameEntity(x)).ToList();
        TagNames = masterGame.Tags.Select(x => x.Name).ToList();
    }

    public Guid MasterGameID { get; set; }
    public string GameName { get; set; } = null!;
    public string EstimatedReleaseDate { get; set; } = null!;
    public LocalDate MinimumReleaseDate { get; set; }
    public LocalDate? MaximumReleaseDate { get; set; }
    public LocalDate? EarlyAccessReleaseDate { get; set; }
    public LocalDate? InternationalReleaseDate { get; set; }
    public LocalDate? AnnouncementDate { get; set; }
    public LocalDate? ReleaseDate { get; set; }
    public int? OpenCriticID { get; set; }
    public string? GGToken { get; set; }
    public string? GGSlug { get; set; }
    public decimal? RawCriticScore { get; set; }
    public bool HasAnyReviews { get; set; }
    public string? OpenCriticSlug { get; set; }
    public string? Notes { get; set; }
    public string? BoxartFileName { get; set; }
    public string? GGCoverArtFileName { get; set; }
    public Instant? FirstCriticScoreTimestamp { get; set; }
    public bool SyncWithExternalAPIs { get; set; }
    public bool UseSimpleEligibility { get; set; }
    public bool DelayContention { get; set; }
    public bool ShowNote { get; set; }
    public Instant AddedTimestamp { get; set; }
    public Guid AddedByUserID { get; set; }
    public string AddedByUserDisplayName { get; set; } = null!;
    public List<MasterSubGameEntity> SubGames { get; set; } = [];
    public List<string> TagNames { get; set; } = [];

    public string ToJson() => JsonSerializer.Serialize(this, FantasyCriticJsonOptions.Default);

    public static MasterGameSnapshotEntity FromJson(string json)
    {
        return JsonSerializer.Deserialize<MasterGameSnapshotEntity>(json, FantasyCriticJsonOptions.Default)
               ?? throw new Exception("A master game snapshot deserialized to null.");
    }

    //tagDictionary is keyed by tag name. A tag deleted since the snapshot was taken throws rather than being dropped.
    public MasterGame ToDomain(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        var tags = TagNames.Select(tagName => tagDictionary.GetValueOrDefault(tagName)
                                              ?? throw new Exception($"The snapshot of {GameName} ({MasterGameID}) has the tag {tagName}, which no longer exists."))
            .ToList();
        var subGames = SubGames.Select(x => x.ToDomain()).ToList();
        var addedByUser = new VeryMinimalFantasyCriticUser(AddedByUserID, AddedByUserDisplayName);
        return new MasterGame(MasterGameID, GameName, EstimatedReleaseDate, MinimumReleaseDate, MaximumReleaseDate, EarlyAccessReleaseDate, InternationalReleaseDate,
            AnnouncementDate, ReleaseDate, OpenCriticID, GGToken, GGSlug, RawCriticScore, HasAnyReviews, OpenCriticSlug, Notes, BoxartFileName, GGCoverArtFileName,
            FirstCriticScoreTimestamp, SyncWithExternalAPIs, UseSimpleEligibility, DelayContention, ShowNote, AddedTimestamp, addedByUser, subGames, tags);
    }
}
