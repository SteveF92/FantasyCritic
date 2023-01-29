using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.Web.Models.Responses;

public class PickupBidViewModel
{
    public PickupBidViewModel(PickupBid pickupBid, LocalDate currentDate, IReadOnlyDictionary<Guid, MasterGameYear> masterGameYearLookup)
    {
        BidID = pickupBid.BidID;
        PublisherID = pickupBid.Publisher.PublisherID;
        PublisherName = pickupBid.Publisher.PublisherName;
        BidAmount = pickupBid.BidAmount;
        AllowIneligibleSlot = pickupBid.AllowIneligibleSlot;
        Priority = pickupBid.Priority;
        Timestamp = pickupBid.Timestamp.ToDateTimeUtc();
        Successful = pickupBid.Successful;

        var masterGameYear = masterGameYearLookup[pickupBid.MasterGame.MasterGameID];
        MasterGame = new MasterGameYearViewModel(masterGameYear, currentDate);

        if (pickupBid.ConditionalDropPublisherGame is not null)
        {
            ConditionalDropPublisherGame = new PublisherGameViewModel(pickupBid.ConditionalDropPublisherGame, currentDate, false, false);
        }

        CounterPick = pickupBid.CounterPick;
        Outcome = pickupBid.Outcome;
        ProjectedPointsAtTimeOfBid = pickupBid.ProjectedPointsAtTimeOfBid;
    }

    public Guid BidID { get; }
    public Guid PublisherID { get; }
    public string PublisherName { get; }
    public uint BidAmount { get; }
    public bool AllowIneligibleSlot { get; }
    public int Priority { get; }
    public DateTime Timestamp { get; }
    public bool? Successful { get; }
    public MasterGameYearViewModel MasterGame { get; }
    public PublisherGameViewModel? ConditionalDropPublisherGame { get; }
    public bool CounterPick { get; }
    public string? Outcome { get; }
    public decimal? ProjectedPointsAtTimeOfBid { get; }
}
