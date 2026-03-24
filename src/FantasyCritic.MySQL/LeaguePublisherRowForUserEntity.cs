namespace FantasyCritic.MySQL;

internal record LeaguePublisherRowForUserEntity(Guid UserID, Guid LeagueID, string LeagueName, int Year, string PublisherName);
