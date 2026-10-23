namespace FantasyCritic.Lib.Domain;

public interface IPublisherGame
{
    Guid PublisherID { get; }
    Guid PublisherGameID { get; }
    string OriginalGameName { get; }
    Instant Timestamp { get; }
    bool CounterPick { get; }
    decimal? ManualCriticScore { get; }
    bool ManualWillNotRelease { get; }
    decimal? FantasyPoints { get; }
    MasterGameYear? MasterGame { get; }
    int? PickNumber { get; }
    int? OverallPickNumber { get; }
    Guid? DraftID { get; }
    uint? BidAmount { get; }
    Guid? AcquiredInTradeID { get; }
    string GameName { get; }
    bool IsFormerPublisherGame { get; }

    WillReleaseStatus WillRelease(LeagueYear leagueYear)
    {
        if (ManualWillNotRelease)
        {
            return WillReleaseStatus.WillNotRelease;
        }

        if (MasterGame is null)
        {
            return WillReleaseStatus.WillRelease;
        }

        var willReleaseStatus = MasterGame.GetWillReleaseStatus();

        var countsAsWillRelease = willReleaseStatus.CountAsWillRelease;
        var plannedForEarlyAccess = (MasterGame?.MasterGame.Tags ?? []).Any(x => x.Name == "PlannedForEarlyAccess" || x.Name == "CurrentlyInEarlyAccess");
        var leagueUsesOnlyNeedsScore = leagueYear.Options.ReleaseSystem.Equals(ReleaseSystem.OnlyNeedsScore);

        if (!countsAsWillRelease && plannedForEarlyAccess && leagueUsesOnlyNeedsScore)
        {
            return WillReleaseStatus.WillReleaseEarlyAccessOnly;
        }

        return willReleaseStatus;
    }

    bool CouldRelease(LeagueYear leagueYear) => WillRelease(leagueYear).CountAsWillRelease;
}
