using System;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.ScoringSystems;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class PickupBidViewModel
    {
        public PickupBidViewModel(PickupBid pickupBid, LocalDate currentDate, ScoringSystem scoringSystem, SystemWideValues systemWideValues)
        {
            BidID = pickupBid.BidID;
            BidAmount = pickupBid.BidAmount;
            Priority = pickupBid.Priority;
            Timestamp = pickupBid.Timestamp.ToDateTimeUtc();
            Successful = pickupBid.Successful;
            MasterGame = new MasterGameViewModel(pickupBid.MasterGame, currentDate);
            ConditionalDropPublisherGame = pickupBid.ConditionalDropPublisherGame.GetValueOrDefault(x => new PublisherGameViewModel(x, currentDate, false, false));
            CounterPick = pickupBid.CounterPick;
        }

        public Guid BidID { get; }
        public uint BidAmount { get; }
        public int Priority { get; }
        public DateTime Timestamp { get; }
        public bool? Successful { get; }
        public MasterGameViewModel MasterGame { get; }
        public PublisherGameViewModel ConditionalDropPublisherGame { get; }
        public bool CounterPick { get; }
    }
}
