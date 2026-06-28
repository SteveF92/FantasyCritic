using NodaTime;

namespace FantasyCritic.Lib.Domain.Requests;

public record DraftParameters(
    string? Name,
    LocalDate? ScheduledDate,
    int GamesToDraft,
    int CounterPicksToDraft);
