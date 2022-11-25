namespace FantasyCritic.MySQL.Entities;

public record LeagueYearHasGameEntity(Guid LeagueID, int year, Guid MasterGameID);
