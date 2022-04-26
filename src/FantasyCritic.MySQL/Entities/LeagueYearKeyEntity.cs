namespace FantasyCritic.MySQL.Entities;
public class LeagueYearKeyEntity
{
    public LeagueYearKeyEntity()
    {

    }

    public LeagueYearKeyEntity(LeagueYearKey domain)
    {
        LeagueID = domain.LeagueID;
        Year = domain.Year;
    }

    public Guid LeagueID { get; set; }
    public int Year { get; set; }
}
