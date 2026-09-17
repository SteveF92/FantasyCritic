namespace FantasyCritic.Lib.Jobs;

public class FantasyCriticJobType : TypeSafeEnum<FantasyCriticJobType>
{
    // Define values here.
    public static readonly FantasyCriticJobType ExpireTrades = new FantasyCriticJobType("ExpireTrades");
    public static readonly FantasyCriticJobType FullDataRefresh = new FantasyCriticJobType("FullDataRefresh");
    public static readonly FantasyCriticJobType GrantSuperDrops = new FantasyCriticJobType("GrantSuperDrops");
    public static readonly FantasyCriticJobType MakeSlotsConsistent = new FantasyCriticJobType("MakeSlotsConsistent");
    public static readonly FantasyCriticJobType PrepareForActionProcessing = new FantasyCriticJobType("PrepareForActionProcessing");
    public static readonly FantasyCriticJobType ProcessActions = new FantasyCriticJobType("ProcessActions");
    public static readonly FantasyCriticJobType ProcessSpecialAuctions = new FantasyCriticJobType("ProcessSpecialAuctions");
    public static readonly FantasyCriticJobType PushGameReleaseMessages = new FantasyCriticJobType("PushGameReleaseMessages");
    public static readonly FantasyCriticJobType SendPublicBiddingDiscordMessages = new FantasyCriticJobType("SendPublicBiddingDiscordMessages");
    public static readonly FantasyCriticJobType SendPublicBiddingEmails = new FantasyCriticJobType("SendPublicBiddingEmails");
    public static readonly FantasyCriticJobType SendAllPublicBiddingMessages = new FantasyCriticJobType("SendAllPublicBiddingMessages");
    public static readonly FantasyCriticJobType RecalculateLastSeasonWinners = new FantasyCriticJobType("RecalculateLastSeasonWinners");
    public static readonly FantasyCriticJobType RecalculateRoyaleWinners = new FantasyCriticJobType("RecalculateRoyaleWinners");
    public static readonly FantasyCriticJobType RecomputeRulesBasedRoyaleGroups = new FantasyCriticJobType("RecomputeRulesBasedRoyaleGroups");
    public static readonly FantasyCriticJobType RefreshCaches = new FantasyCriticJobType("RefreshCaches");
    public static readonly FantasyCriticJobType RefreshCriticScores = new FantasyCriticJobType("RefreshCriticScores");
    public static readonly FantasyCriticJobType RefreshGGInfo = new FantasyCriticJobType("RefreshGGInfo");
    public static readonly FantasyCriticJobType RefreshPatreonInfo = new FantasyCriticJobType("RefreshPatreonInfo");
    public static readonly FantasyCriticJobType SendReleasingThisWeekUpdate = new FantasyCriticJobType("SendReleasingThisWeekUpdate");
    public static readonly FantasyCriticJobType SetTimeFlags = new FantasyCriticJobType("SetTimeFlags");
    public static readonly FantasyCriticJobType SnapshotDatabase = new FantasyCriticJobType("SnapshotDatabase");
    public static readonly FantasyCriticJobType UpdateDailyPublisherStatistics = new FantasyCriticJobType("UpdateDailyPublisherStatistics");
    public static readonly FantasyCriticJobType UpdateFantasyPoints = new FantasyCriticJobType("UpdateFantasyPoints");
    public static readonly FantasyCriticJobType UpdateTopBidsAndDrops = new FantasyCriticJobType("UpdateTopBidsAndDrops");

    // Constructor is private: values are defined within this class only!
    //Handlers, and the schedules of cron handlers, are in FantasyCriticJobRegistry. The database owns whether each may run (tbl_job_type.RunType).
    private FantasyCriticJobType(string value)
        : base(value)
    {

    }

    public override string ToString() => Value;
}
