namespace FantasyCritic.MySQL.Entities;
public class SpecialAuctionEntity
{
    public SpecialAuctionEntity()
    {

    }

    public SpecialAuctionEntity(SpecialAuction domain)
    {
        SpecialAuctionID = domain.SpecialAuctionID;
        LeagueID = domain.LeagueYearKey.LeagueID;
        Year = domain.LeagueYearKey.Year;
        MasterGameID = domain.MasterGameYear.MasterGame.MasterGameID;
        CreationTime = domain.CreationTime;
        ScheduledEndTime = domain.ScheduledEndTime;
        Processed = domain.Processed;
    }

    public Guid SpecialAuctionID { get; set; }
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public Guid MasterGameID { get; set; }
    public Instant CreationTime { get; set; }
    public Instant ScheduledEndTime { get; set; }
    public bool Processed { get; set; }

    public SpecialAuction ToDomain(MasterGameYear masterGameYear)
    {
        return new SpecialAuction(SpecialAuctionID, new LeagueYearKey(LeagueID, Year), masterGameYear, CreationTime, ScheduledEndTime,
            Processed);
    }
}
