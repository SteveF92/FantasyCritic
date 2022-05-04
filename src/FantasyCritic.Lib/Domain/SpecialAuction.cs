namespace FantasyCritic.Lib.Domain;
public class SpecialAuction
{
    public SpecialAuction(LeagueYearKey leagueYearKey, MasterGameYear masterGameYear, Instant creationTime,
        Instant scheduledEndTime, bool processed)
    {
        LeagueYearKey = leagueYearKey;
        MasterGameYear = masterGameYear;
        CreationTime = creationTime;
        ScheduledEndTime = scheduledEndTime;
        Processed = processed;
    }

    public LeagueYearKey LeagueYearKey { get; }
    public MasterGameYear MasterGameYear { get; }
    public Instant CreationTime { get; }
    public Instant ScheduledEndTime { get; }
    public bool Processed { get; }

    public bool IsLocked(Instant currentInstant) => currentInstant >= ScheduledEndTime;
}
