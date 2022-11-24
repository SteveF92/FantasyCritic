using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Requests.Admin;

public class CreateMasterGameRequest
{
    public CreateMasterGameRequest(string gameName, string estimatedReleaseDate)
    {
        GameName = gameName;
        EstimatedReleaseDate = estimatedReleaseDate;
    }

    public string GameName { get; }
    public string EstimatedReleaseDate { get; }

    public List<string> Tags { get; init; } = new List<string>();
    public bool DoNotRefreshDate { get; init; } = false;
    public bool DoNotRefreshAnything { get; init; } = false;
    public bool EligibilityChanged { get; init; } = false;
    public bool DelayContention { get; init; } = false;
    public bool ShowNote { get; init; } = false;

    public LocalDate? MinimumReleaseDate { get; init; }
    public LocalDate? MaximumReleaseDate { get; init; }
    public LocalDate? EarlyAccessReleaseDate { get; init; }
    public LocalDate? InternationalReleaseDate { get; init; }
    public LocalDate? AnnouncementDate { get; init; }
    public LocalDate? ReleaseDate { get; init; }
    public int? OpenCriticID { get; init; }
    public string? GGToken { get; init; }
    public string? Notes { get; init; }

    public Lib.Domain.MasterGame ToDomain(IClock clock, IEnumerable<MasterGameTag> tags, FantasyCriticUser addedByUser)
    {
        var now = clock.GetCurrentInstant();
        LocalDate minimumReleaseDate = now.ToEasternDate().PlusDays(1);
        if (MinimumReleaseDate.HasValue)
        {
            minimumReleaseDate = MinimumReleaseDate.Value;
        }
        Lib.Domain.MasterGame masterGame = new Lib.Domain.MasterGame(Guid.NewGuid(), GameName, EstimatedReleaseDate, minimumReleaseDate, MaximumReleaseDate,
            EarlyAccessReleaseDate, InternationalReleaseDate, AnnouncementDate, ReleaseDate, OpenCriticID, GGToken, null, false, null, Notes, null, null, null,
            DoNotRefreshDate, DoNotRefreshAnything, EligibilityChanged, DelayContention, ShowNote, now, addedByUser, new List<MasterSubGame>(), tags);
        return masterGame;
    }
}
