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

    public List<string> Tags { get; init; } = new List<string>();
    public bool DoNotRefreshDate { get; init; } = false;
    public bool DoNotRefreshAnything { get; init; } = false;
    public bool EligibilityChanged { get; init; } = false;
    public bool DelayContention { get; init; } = false;

    public LocalDate? MaximumReleaseDate { get; init; }
    public LocalDate? EarlyAccessReleaseDate { get; init; }
    public LocalDate? InternationalReleaseDate { get; init; }
    public LocalDate? AnnouncementDate { get; init; }
    public LocalDate? ReleaseDate { get; init; }
    public int? OpenCriticID { get; init; }
    public string? GGToken { get; init; }
    public string? Notes { get; init; }

    public Lib.Domain.MasterGame ToDomain(Lib.Domain.MasterGame existingMasterGame, Instant timestamp, IEnumerable<MasterGameTag> tags)
    {
        var masterGame = new Lib.Domain.MasterGame(MasterGameID, GameName, EstimatedReleaseDate, MinimumReleaseDate, MaximumReleaseDate,
            EarlyAccessReleaseDate, InternationalReleaseDate, AnnouncementDate, ReleaseDate, OpenCriticID, GGToken, existingMasterGame.RawCriticScore, existingMasterGame.HasAnyReviews, Notes, existingMasterGame.BoxartFileName,
            existingMasterGame.GGCoverArtFileName, existingMasterGame.FirstCriticScoreTimestamp, DoNotRefreshDate, DoNotRefreshAnything, EligibilityChanged, DelayContention,
            timestamp, existingMasterGame.AddedByUser, existingMasterGame.SubGames, tags);
        return masterGame;
    }
}
