using FantasyCritic.Lib.Domain.Requests;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record EditLeagueDraftRequest(
    Guid DraftID,
    Guid LeagueID,
    int Year,
    string Name,
    LocalDate? ScheduledDate,
    int GamesToDraft,
    int CounterPicksToDraft)
{
    public EditLeagueDraftParameters ToDomain()
    {
        return new EditLeagueDraftParameters(new LeagueYearKey(LeagueID, Year), Name, ScheduledDate, GamesToDraft, CounterPicksToDraft);
    }
}
