using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.MySQL.Entities;

public class PickupBidEntity
{
    public PickupBidEntity()
    {

    }

    public PickupBidEntity(PickupBid domain)
    {
        BidID = domain.BidID;
        PublisherID = domain.Publisher.PublisherID;
        MasterGameID = domain.MasterGame.MasterGameID;
        ConditionalDropMasterGameID = domain.ConditionalDropPublisherGame.GetValueOrDefault<PublisherGame, Guid?>(x => x.MasterGame.ValueTempoTemp.MasterGame.MasterGameID);
        CounterPick = domain.CounterPick;
        Timestamp = domain.Timestamp;
        Priority = domain.Priority;
        BidAmount = domain.BidAmount;
        Successful = domain.Successful;
        ProcessSetID = domain.ProcessSetID;
        Outcome = domain.Outcome.GetValueOrDefault();
        ProjectedPointsAtTimeOfBid = domain.ProjectedPointsAtTimeOfBid;
    }

    public PickupBidEntity(IProcessedBid domain, bool successful, Guid processSetID)
    {
        BidID = domain.PickupBid.BidID;
        PublisherID = domain.PickupBid.Publisher.PublisherID;
        MasterGameID = domain.PickupBid.MasterGame.MasterGameID;
        ConditionalDropMasterGameID = domain.PickupBid.ConditionalDropPublisherGame.GetValueOrDefault<PublisherGame, Guid?>(x => x.MasterGame.ValueTempoTemp.MasterGame.MasterGameID);
        CounterPick = domain.PickupBid.CounterPick;
        Timestamp = domain.PickupBid.Timestamp;
        Priority = domain.PickupBid.Priority;
        BidAmount = domain.PickupBid.BidAmount;
        Successful = successful;
        ProcessSetID = processSetID;
        Outcome = domain.Outcome;
        ProjectedPointsAtTimeOfBid = domain.ProjectedPointsAtTimeOfBid;
    }

    public PickupBidEntity(PickupBid domain, Maybe<PublisherGame> conditionalDropPublisherGame, uint bidAmount) : this(domain)
    {
        BidID = domain.BidID;
        PublisherID = domain.Publisher.PublisherID;
        MasterGameID = domain.MasterGame.MasterGameID;
        ConditionalDropMasterGameID = conditionalDropPublisherGame.GetValueOrDefault<PublisherGame, Guid?>(x => x.MasterGame.ValueTempoTemp.MasterGame.MasterGameID);
        CounterPick = domain.CounterPick;
        Timestamp = domain.Timestamp;
        Priority = domain.Priority;
        BidAmount = bidAmount;
        Successful = domain.Successful;
        ProcessSetID = domain.ProcessSetID;
        Outcome = domain.Outcome.GetValueOrDefault();
        ProjectedPointsAtTimeOfBid = domain.ProjectedPointsAtTimeOfBid;
    }

    public Guid BidID { get; set; }
    public Guid PublisherID { get; set; }
    public Guid MasterGameID { get; set; }
    public Guid? ConditionalDropMasterGameID { get; set; }
    public bool CounterPick { get; set; }
    public Instant Timestamp { get; set; }
    public int Priority { get; set; }
    public uint BidAmount { get; set; }
    public bool? Successful { get; set; }
    public Guid? ProcessSetID { get; set; }
    public string Outcome { get; set; }
    public decimal? ProjectedPointsAtTimeOfBid { get; set; }

    public PickupBid ToDomain(Publisher publisher, MasterGame masterGame, Maybe<PublisherGame> conditionalDropPublisherGame, LeagueYear leagueYear)
    {
        return new PickupBid(BidID, publisher, leagueYear, masterGame, conditionalDropPublisherGame, CounterPick, BidAmount, Priority, Timestamp, Successful, ProcessSetID, Outcome.ToMaybe(), ProjectedPointsAtTimeOfBid);
    }
}
