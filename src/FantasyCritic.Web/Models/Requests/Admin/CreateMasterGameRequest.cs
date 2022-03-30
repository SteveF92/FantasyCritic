using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Admin;

public class CreateMasterGameRequest
{
    public CreateMasterGameRequest(string gameName, string estimatedReleaseDate, LocalDate minimumReleaseDate)
    {
        GameName = gameName;
        EstimatedReleaseDate = estimatedReleaseDate;
        MinimumReleaseDate = minimumReleaseDate;
        Tags = new List<string>();
    }

    public string GameName { get; }
    public string EstimatedReleaseDate { get; }
    public LocalDate MinimumReleaseDate { get; }
    public List<string> Tags { get; init; }

    public LocalDate? MaximumReleaseDate { get; init; }
    public LocalDate? EarlyAccessReleaseDate { get; init; }
    public LocalDate? InternationalReleaseDate { get; init; }
    public LocalDate? AnnouncementDate { get; init; }
    public LocalDate? ReleaseDate { get; init; }
    public int? OpenCriticID { get; init; }
    public string? GGToken { get; init; }
    public string? Notes { get; init; }
    
    public Lib.Domain.MasterGame ToDomain(Instant timestamp, IEnumerable<MasterGameTag> tags)
    {
        Lib.Domain.MasterGame masterGame = new Lib.Domain.MasterGame(Guid.NewGuid(), GameName, EstimatedReleaseDate, MinimumReleaseDate, MaximumReleaseDate,
            EarlyAccessReleaseDate, InternationalReleaseDate, AnnouncementDate, ReleaseDate, OpenCriticID, GGToken, null, Notes, "", "",
            null, false, false, false, false, timestamp, new List<MasterSubGame>(), tags);
        return masterGame;
    }
}
