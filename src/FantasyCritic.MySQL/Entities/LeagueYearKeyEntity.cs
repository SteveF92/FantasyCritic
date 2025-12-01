namespace FantasyCritic.MySQL.Entities;

internal class LeagueYearKeyEntity
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

internal class LeagueYearKeyWithDetailsEntity
{
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public bool SupportedYearIsFinished { get; set; }
    public bool DraftStarted { get; set; }
}
