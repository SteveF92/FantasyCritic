namespace FantasyCritic.MySQL.Entities;
internal class LeagueYearStatusEntity
{
    public LeagueYearStatusEntity()
    {
        
    }

    public LeagueYearStatusEntity(int year, string playStatus)
    {
        Year = year;
        PlayStatus = playStatus;
    }

    public int Year { get; set; }
    public string PlayStatus { get; set; } = null!;
}
