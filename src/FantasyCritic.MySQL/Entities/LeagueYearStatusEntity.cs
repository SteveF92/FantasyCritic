namespace FantasyCritic.MySQL.Entities;
internal class LeagueYearStatusEntity
{
    public LeagueYearStatusEntity()
    {

    }

    public LeagueYearStatusEntity(int year, bool anyDraftStarted)
    {
        Year = year;
        AnyDraftStarted = anyDraftStarted;
    }

    public int Year { get; set; }
    public bool AnyDraftStarted { get; set; }
}
