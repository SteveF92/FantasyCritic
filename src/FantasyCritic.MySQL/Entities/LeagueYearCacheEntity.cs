namespace FantasyCritic.MySQL.Entities;
public class LeagueYearCacheEntity
{
    public LeagueYearCacheEntity()
    {
        
    }

    public LeagueYearCacheEntity(LeagueYear domain)
    {
        LeagueID = domain.League.LeagueID;
        Year = domain.Year;
        OneShotMode = domain.Options.OneShotMode;
    }

    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public bool OneShotMode { get; set; }
}
