namespace FantasyCritic.MySQL.Entities;

public class UserActiveLeaguesEntity
{
    public required Guid UserID { get; init; }
    public required Guid LeagueID { get; init; }
    public required int Year { get; init; }
}
