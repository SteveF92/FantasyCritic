namespace FantasyCritic.MySQL.Entities;
internal class PublicLeagueYearStatsEntity
{
    public Guid LeagueID { get; set; }
    public string LeagueName { get; set; } = null!;
    public int NumberOfFollowers { get; set; }
    public string PlayStatus { get; set; } = null!;

    public PublicLeagueYearStats ToDomain()
    {
        return new PublicLeagueYearStats(LeagueID, LeagueName, NumberOfFollowers, Lib.Enums.PlayStatus.FromValue(PlayStatus));
    }
}
