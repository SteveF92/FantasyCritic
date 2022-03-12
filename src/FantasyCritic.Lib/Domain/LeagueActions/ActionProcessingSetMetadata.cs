namespace FantasyCritic.Lib.Domain.LeagueActions;

public record ActionProcessingSetMetadata(Guid ProcessSetID, Instant ProcessTime, string ProcessName);
