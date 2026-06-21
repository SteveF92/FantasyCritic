namespace FantasyCritic.Lib.Domain;

public record PublicLeagueYearStats(Guid LeagueID, string LeagueName, int NumberOfFollowers, bool AnyDraftStarted);
