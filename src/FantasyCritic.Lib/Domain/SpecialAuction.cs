namespace FantasyCritic.Lib.Domain;
public class SpecialAuction
{
    public SpecialAuction(LeagueYearKey leagueYearKey, MasterGame masterGame, Instant creationTime,
        Instant scheduledEndTime, bool processed)
    {
        LeagueYearKey = leagueYearKey;
        MasterGame = masterGame;
        CreationTime = creationTime;
        ScheduledEndTime = scheduledEndTime;
        Processed = processed;
    }

    public LeagueYearKey LeagueYearKey { get; }
    public MasterGame MasterGame { get; }
    public Instant CreationTime { get; }
    public Instant ScheduledEndTime { get; }
    public bool Processed { get; }
}
