using FantasyCritic.Lib.Domain.Calculations;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.ScoringSystems;

namespace FantasyCritic.Lib.Domain;

public class PublisherGame : IEquatable<PublisherGame>
{
    public PublisherGame(Guid publisherID, Guid publisherGameID, string gameName, Instant timestamp, bool counterPick, decimal? manualCriticScore, bool manualWillNotRelease,
        decimal? fantasyPoints, MasterGameYear? masterGame, int slotNumber, int? draftPosition, int? overallDraftPosition, uint? bidAmount, Guid? acquiredInTradeID)
    {
        PublisherID = publisherID;
        PublisherGameID = publisherGameID;
        OriginalGameName = gameName;
        Timestamp = timestamp;
        CounterPick = counterPick;
        ManualCriticScore = manualCriticScore;
        ManualWillNotRelease = manualWillNotRelease;
        FantasyPoints = fantasyPoints;
        MasterGame = masterGame;
        SlotNumber = slotNumber;
        DraftPosition = draftPosition;
        OverallDraftPosition = overallDraftPosition;
        BidAmount = bidAmount;
        AcquiredInTradeID = acquiredInTradeID;
    }

    public Guid PublisherID { get; }
    public Guid PublisherGameID { get; }
    public string OriginalGameName { get; }
    public Instant Timestamp { get; }
    public bool CounterPick { get; }
    public decimal? ManualCriticScore { get; }
    public bool ManualWillNotRelease { get; }
    public decimal? FantasyPoints { get; }
    public MasterGameYear? MasterGame { get; }
    public int SlotNumber { get; }
    public int? DraftPosition { get; }
    public int? OverallDraftPosition { get; }
    public uint? BidAmount { get; }
    public Guid? AcquiredInTradeID { get; }

    public string GameName => MasterGame?.MasterGame.GameName ?? OriginalGameName;

    public WillReleaseStatus WillRelease()
    {
        if (ManualWillNotRelease)
        {
            return WillReleaseStatus.WillNotRelease;
        }

        if (MasterGame is null)
        {
            return WillReleaseStatus.WillRelease;
        }

        return MasterGame.GetWillReleaseStatus();
    }

    public bool CouldRelease() => WillRelease().CountAsWillRelease;

    public PublisherGame GetUpdatedPublisherGameWithNewScores(IReadOnlyDictionary<Guid, PublisherGameCalculatedStats> calculatedStats)
    {
        if (!calculatedStats.TryGetValue(PublisherGameID, out var stats))
        {
            return this;
        }

        return new PublisherGame(PublisherID, PublisherGameID, OriginalGameName, Timestamp, CounterPick, ManualCriticScore, ManualWillNotRelease,
            stats.FantasyPoints, MasterGame, SlotNumber, DraftPosition, OverallDraftPosition, BidAmount, AcquiredInTradeID);
    }

    public FormerPublisherGame GetFormerPublisherGame(Instant removedTimestamp, string removedNote) => new FormerPublisherGame(this, removedTimestamp, removedNote);
    public MasterGameYearWithCounterPick? GetMasterGameYearWithCounterPick()
    {
        if (MasterGame is null)
        {
            return null;
        }

        return new MasterGameYearWithCounterPick(MasterGame, CounterPick);
    }

    public double GetSleeperFactor(ScoringSystem scoringSystem)
    {
        double minFantasyPoints = (double)scoringSystem.GetMinimumScore();
        double maxFantasyPoints = (double)scoringSystem.GetMaximumScore();

        var hypeFactor = MasterGame!.DateAdjustedHypeFactor;
        var fantasyPoints = (double)(FantasyPoints ?? 0);

        // Ensure FantasyPoints are within the valid range
        fantasyPoints = Math.Clamp(fantasyPoints, minFantasyPoints, maxFantasyPoints);

        // Sleeper Factor formula
        double sleeperFactor = (fantasyPoints * (100 - hypeFactor)) / (maxFantasyPoints - minFantasyPoints);

        return sleeperFactor;
    }

    
    public double GetFlopFactor(ScoringSystem scoringSystem)
    {
        double minFantasyPoints = (double)scoringSystem.GetMinimumScore();
        double maxFantasyPoints = (double)scoringSystem.GetMaximumScore();

        var hypeFactor = MasterGame!.DateAdjustedHypeFactor;
        var fantasyPoints = (double) (FantasyPoints ?? 0);
        if (hypeFactor <= 0)
        {
            return 0;
        }

        // Constants
        const double effectiveThreshold = 12.0; // "Excellent" threshold for FantasyPoints

        // Ensure FantasyPoints are within valid range
        fantasyPoints = Math.Clamp(fantasyPoints, minFantasyPoints, maxFantasyPoints);

        // Calculate disappointment factor
        double disappointmentFactor = hypeFactor * Math.Max(0, (1 - (fantasyPoints / effectiveThreshold)));

        return disappointmentFactor;
    }

    public double GetDraftValue(LeagueYear leagueYear)
    {
        var scoringSystem = leagueYear.Options.ScoringSystem;
        if (!DraftPosition.HasValue || DraftPosition.Value <= 0)
        {
            return 0; // No valid draft position
        }

        double minFantasyPoints = (double)scoringSystem.GetMinimumScore();
        double maxFantasyPoints = (double)scoringSystem.GetMaximumScore();

        var fantasyPoints = (double)(FantasyPoints ?? 0);

        // Ensure FantasyPoints are within the valid range
        fantasyPoints = Math.Clamp(fantasyPoints, minFantasyPoints, maxFantasyPoints);

        double draftValue = fantasyPoints * DraftPosition.Value;
        return draftValue;
    }


    public double GetBidValue(ScoringSystem scoringSystem)
    {
        if (!BidAmount.HasValue)
        {
            return 0; // No valid bid amount
        }

        double minFantasyPoints = (double)scoringSystem.GetMinimumScore();
        double maxFantasyPoints = (double)scoringSystem.GetMaximumScore();

        var fantasyPoints = (double)(FantasyPoints ?? 0);

        // Ensure FantasyPoints are within the valid range
        fantasyPoints = Math.Clamp(fantasyPoints, minFantasyPoints, maxFantasyPoints);

        // Convert BidAmount to a double and ensure it's at least 0.5 if BidAmount is 0
        double bidAmount = (double)BidAmount.Value;
        bidAmount = bidAmount == 0 ? 0.5 : bidAmount;  // Convert 0 bid to 0.5

        // Bid Value = FantasyPoints / BidAmount
        double bidValue = fantasyPoints / bidAmount;

        return bidValue;
    }

    public double GetDollarsPerPoint(ScoringSystem scoringSystem)
    {
        if (!BidAmount.HasValue)
        {
            return 0; // No valid bid amount
        }

        if (FantasyPoints is null || FantasyPoints <= 0)
        {
            return 0;
        }

        double minFantasyPoints = (double)scoringSystem.GetMinimumScore();
        double maxFantasyPoints = (double)scoringSystem.GetMaximumScore();

        var fantasyPoints = (double)(FantasyPoints ?? 0);

        // Ensure FantasyPoints are within the valid range
        fantasyPoints = Math.Clamp(fantasyPoints, minFantasyPoints, maxFantasyPoints);

        // Convert BidAmount to a double and ensure it's at least 0.5 if BidAmount is 0
        double bidAmount = (double)BidAmount.Value;
        bidAmount = bidAmount == 0 ? 0.5 : bidAmount;  // Convert 0 bid to 0.5

        // Bid Value = FantasyPoints / BidAmount
        double bidValue = bidAmount / fantasyPoints;
        return bidValue;
    }

    public override string ToString() => GameName;

    public bool Equals(PublisherGame? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return PublisherGameID.Equals(other.PublisherGameID);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((PublisherGame) obj);
    }

    public override int GetHashCode()
    {
        return PublisherGameID.GetHashCode();
    }
}
