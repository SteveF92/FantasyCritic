namespace FantasyCritic.MySQL.Entities;

internal class LeagueYearsWithMasterGameEntity
{
    public LeagueYearsWithMasterGameEntity()
    {
    }

    public LeagueYearsWithMasterGameEntity(Guid leagueID, string leagueName, int year, bool isCounterPick)
    {
        Year = year;
        LeagueID = leagueID;
        LeagueName = leagueName;
        CounterPick = isCounterPick;
    }
    public Guid LeagueID { get; set; }
    public string LeagueName { get; set; } = null!;
    public int Year { get; set; }
    public bool CounterPick { get; set; }

    public LeagueYearWithMasterGame ToDomain()
    {
        return new LeagueYearWithMasterGame(LeagueID, LeagueName, Year, CounterPick);
    }
}
