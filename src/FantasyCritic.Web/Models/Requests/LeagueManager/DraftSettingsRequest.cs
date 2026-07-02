using FantasyCritic.Lib.Domain.Requests;
using NodaTime;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record DraftSettingsRequest(
    string? Name,
    LocalDate? ScheduledDate,
    int GamesToDraft,
    int CounterPicksToDraft,
    bool CounterPicksMustBeFromThisDraft = true)
{
    public DraftParameters ToDomain(int draftIndex)
    {
        string resolvedName = Name ?? (draftIndex == 0 ? "Initial Draft" : $"Draft {draftIndex + 1}");
        return new DraftParameters(resolvedName, ScheduledDate, GamesToDraft, CounterPicksToDraft, CounterPicksMustBeFromThisDraft);
    }
}
