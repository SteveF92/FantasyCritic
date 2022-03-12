using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.Web.Models.Responses;

public class PickupBidViewModel
{
    public PickupBidViewModel(PickupBid pickupBid, LocalDate currentDate)
    {
        BidID = pickupBid.BidID;
        PublisherID = pickupBid.Publisher.PublisherID;
        PublisherName = pickupBid.Publisher.PublisherName;
        BidAmount = pickupBid.BidAmount;
        Priority = pickupBid.Priority;
        Timestamp = pickupBid.Timestamp.ToDateTimeUtc();
        Successful = pickupBid.Successful;
        MasterGame = new MasterGameViewModel(pickupBid.MasterGame, currentDate);
        ConditionalDropPublisherGame = pickupBid.ConditionalDropPublisherGame.GetValueOrDefault(x => new PublisherGameViewModel(x, currentDate, false, false));
        CounterPick = pickupBid.CounterPick;
        Outcome = pickupBid.Outcome.GetValueOrDefault();
        ProjectedPointsAtTimeOfBid = pickupBid.ProjectedPointsAtTimeOfBid;
    }

    public Guid BidID { get; }
    public Guid PublisherID { get; }
    public string PublisherName { get; }
    public uint BidAmount { get; }
    public int Priority { get; }
    public DateTime Timestamp { get; }
    public bool? Successful { get; }
    public MasterGameViewModel MasterGame { get; }
    public PublisherGameViewModel ConditionalDropPublisherGame { get; }
    public bool CounterPick { get; }
    public string Outcome { get; }
    public decimal? ProjectedPointsAtTimeOfBid { get; }
}