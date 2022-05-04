namespace FantasyCritic.MySQL.Entities;
public class SpecialAuctionEntity
{
    public SpecialAuctionEntity()
    {

    }

    public SpecialAuctionEntity(SpecialAuction domain)
    {
        LeagueID = domain.LeagueYearKey.LeagueID;
        Year = domain.LeagueYearKey.Year;
        MasterGameID = domain.MasterGame.MasterGameID;
        CreationTime = domain.CreationTime;
        ScheduledEndTime = domain.ScheduledEndTime;
        Processed = domain.Processed;
    }

    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public Guid MasterGameID { get; set; }
    public Instant CreationTime { get; set; }
    public Instant ScheduledEndTime { get; set; }
    public bool Processed { get; set; }

    public SpecialAuction ToDomain(MasterGame masterGame)
    {
        return new SpecialAuction(new LeagueYearKey(LeagueID, Year), masterGame, CreationTime, ScheduledEndTime,
            Processed);
    }
}
