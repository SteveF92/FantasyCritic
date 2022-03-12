namespace FantasyCritic.MySQL.Entities;

public class UserActiveLeaguesEntity
{
    public Guid UserID { get; set; }
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
}
