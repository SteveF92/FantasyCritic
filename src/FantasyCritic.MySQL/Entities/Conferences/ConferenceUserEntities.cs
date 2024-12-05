namespace FantasyCritic.MySQL.Entities.Conferences;
internal class LeagueManagerEntity
{
    public Guid LeagueID { get; set; }
    public Guid LeagueManager { get; set; }
}

internal class LeagueUserEntity
{
    public Guid LeagueID { get; set; }
    public Guid UserID { get; set; }
}

internal class LeagueActivePlayerEntity
{
    public Guid LeagueID { get; set; }
    public Guid UserID { get; set; }
    public int Year { get; set; }
}

internal class ConferenceActivePlayerEntity
{
    public Guid ConferenceID { get; set; }
    public Guid UserID { get; set; }
    public int Year { get; set; }
}
