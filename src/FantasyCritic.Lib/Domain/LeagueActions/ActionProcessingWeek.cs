namespace FantasyCritic.Lib.Domain.LeagueActions;

public record ActionProcessingWeek(LocalDate ProcessDate, IReadOnlyList<ActionProcessingSetMetadata> ProcessingSets);
