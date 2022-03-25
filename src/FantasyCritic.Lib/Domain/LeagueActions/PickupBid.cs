using FantasyCritic.Lib.Domain.Results;

namespace FantasyCritic.Lib.Domain.LeagueActions;

public class PickupBid : IEquatable<PickupBid>
{
    public PickupBid(Guid bidID, Publisher publisher, LeagueYear leagueYear, MasterGame masterGame, Maybe<PublisherGame> conditionalDropPublisherGame,
        bool counterPick, uint bidAmount, int priority, Instant timestamp, bool? successful, Guid? processSetID, Maybe<string> outcome, decimal? projectedPointsAtTimeOfBid)
    {
        BidID = bidID;
        Publisher = publisher;
        LeagueYear = leagueYear;
        MasterGame = masterGame;
        ConditionalDropPublisherGame = conditionalDropPublisherGame;
        CounterPick = counterPick;
        BidAmount = bidAmount;
        Priority = priority;
        Timestamp = timestamp;
        Successful = successful;
        ProcessSetID = processSetID;
        Outcome = outcome;
        ProjectedPointsAtTimeOfBid = projectedPointsAtTimeOfBid;
        ConditionalDropResult = Maybe<DropResult>.None;
    }

    public PickupBid(Guid bidID, Publisher publisher, LeagueYear leagueYear, MasterGame masterGame, Maybe<PublisherGame> conditionalDropPublisherGame,
        bool counterPick, uint bidAmount, int priority, Instant timestamp, bool? successful, Guid? processSetID, Maybe<string> outcome, decimal? projectedPointsAtTimeOfBid, DropResult conditionalDropResult)
    :this(bidID, publisher, leagueYear, masterGame, conditionalDropPublisherGame, counterPick, bidAmount, priority, timestamp, successful, processSetID, outcome, projectedPointsAtTimeOfBid)
    {
        ConditionalDropResult = conditionalDropResult;
    }

    public Guid BidID { get; }
    public Publisher Publisher { get; }
    public LeagueYear LeagueYear { get; }
    public MasterGame MasterGame { get; }
    public Maybe<PublisherGame> ConditionalDropPublisherGame { get; }
    public bool CounterPick { get; }
    public uint BidAmount { get; }
    public int Priority { get; }
    public Instant Timestamp { get; }
    public bool? Successful { get; }
    public Guid? ProcessSetID { get; }
    public Maybe<string> Outcome { get; }
    public decimal? ProjectedPointsAtTimeOfBid { get; }

    public Maybe<DropResult> ConditionalDropResult { get; }

    public PickupBid WithConditionalDropResult(DropResult conditionalDropResult)
    {
        return new PickupBid(BidID, Publisher, LeagueYear, MasterGame, ConditionalDropPublisherGame, CounterPick,
            BidAmount, Priority, Timestamp, Successful, ProcessSetID, Outcome, ProjectedPointsAtTimeOfBid,
            conditionalDropResult);
    }

    public bool Equals(PickupBid other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return BidID.Equals(other.BidID);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((PickupBid)obj);
    }

    public override int GetHashCode()
    {
        return BidID.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Publisher.PublisherName}|{MasterGame.GameName}|{CounterPick}|{BidAmount}|{Priority}|{Successful}";
    }
}
