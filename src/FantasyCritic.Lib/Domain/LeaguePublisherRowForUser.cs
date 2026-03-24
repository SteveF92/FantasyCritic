namespace FantasyCritic.Lib.Domain;

public record LeaguePublisherRowForUser(Guid UserID, Guid LeagueID, string LeagueName, int Year, string PublisherName);
