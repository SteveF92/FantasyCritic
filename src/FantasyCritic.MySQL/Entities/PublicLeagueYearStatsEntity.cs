namespace FantasyCritic.MySQL.Entities;
internal class PublicLeagueYearStatsEntity
{
    public Guid LeagueID { get; set; }
    public string LeagueName { get; set; } = null!;
    public int NumberOfFollowers { get; set; }
    public bool AnyDraftStarted { get; set; }

    public PublicLeagueYearStats ToDomain()
    {
        return new PublicLeagueYearStats(LeagueID, LeagueName, NumberOfFollowers, AnyDraftStarted);
    }
}
