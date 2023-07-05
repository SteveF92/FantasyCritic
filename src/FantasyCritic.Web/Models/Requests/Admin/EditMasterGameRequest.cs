namespace FantasyCritic.Web.Models.Requests.Admin;

public class EditMasterGameRequest
{
    public EditMasterGameRequest(Guid masterGameID, string gameName, string estimatedReleaseDate, LocalDate minimumReleaseDate)
    {
        MasterGameID = masterGameID;
        GameName = gameName;
        EstimatedReleaseDate = estimatedReleaseDate;
        MinimumReleaseDate = minimumReleaseDate;
    }

    public Guid MasterGameID { get; }
    public string GameName { get; }
    public string EstimatedReleaseDate { get; }
    public LocalDate MinimumReleaseDate { get; }

    public required List<string> Tags { get; init; }
    public bool DoNotRefreshDate { get; init; }
    public bool DoNotRefreshAnything { get; init; }
    public bool UseSimpleEligibility { get; init; }
    public bool DelayContention { get; init; }
    public bool ShowNote { get; init; }

    public LocalDate? MaximumReleaseDate { get; init; }
    public LocalDate? EarlyAccessReleaseDate { get; init; }
    public LocalDate? InternationalReleaseDate { get; init; }
    public LocalDate? AnnouncementDate { get; init; }
    public LocalDate? ReleaseDate { get; init; }
    public int? OpenCriticID { get; init; }
    public string? GGToken { get; init; }
    public string? Notes { get; init; }
    public bool MinorEdit { get; init; }

    public Lib.Domain.MasterGame ToDomain(Lib.Domain.MasterGame existingMasterGame, Instant timestamp, IEnumerable<MasterGameTag> tags)
    {
        var masterGame = new Lib.Domain.MasterGame(MasterGameID, GameName, EstimatedReleaseDate, MinimumReleaseDate, MaximumReleaseDate,
            EarlyAccessReleaseDate, InternationalReleaseDate, AnnouncementDate, ReleaseDate, OpenCriticID, GGToken, existingMasterGame.GGSlug, existingMasterGame.RawCriticScore, existingMasterGame.HasAnyReviews, existingMasterGame.OpenCriticSlug,
            Notes, existingMasterGame.BoxartFileName, existingMasterGame.GGCoverArtFileName, existingMasterGame.FirstCriticScoreTimestamp,
            DoNotRefreshDate, DoNotRefreshAnything, UseSimpleEligibility, DelayContention, ShowNote,
            timestamp, existingMasterGame.AddedByUser, existingMasterGame.SubGames, tags);
        return masterGame;
    }
}
