namespace FantasyCritic.MySQL.Entities;
internal class LeagueHasUserEntity
{
    public Guid LeagueID { get; set; }
    public Guid UserID { get; set; }
}

internal class LeagueYearActivePlayer
{
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public Guid UserID { get; set; }
}
