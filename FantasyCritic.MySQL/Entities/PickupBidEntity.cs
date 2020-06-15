using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.MySQL.Entities
{
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
            Timestamp = domain.Timestamp.ToDateTimeUtc();
            Priority = domain.Priority;
            BidAmount = domain.BidAmount;
            Successful = domain.Successful;
        }

        public PickupBidEntity(PickupBid domain, bool successful)
        {
            BidID = domain.BidID;
            PublisherID = domain.Publisher.PublisherID;
            MasterGameID = domain.MasterGame.MasterGameID;
            Timestamp = domain.Timestamp.ToDateTimeUtc();
            Priority = domain.Priority;
            BidAmount = domain.BidAmount;
            Successful = successful;
        }

        public Guid BidID { get; set; }
        public Guid PublisherID { get; set; }
        public Guid MasterGameID { get; set; }
        public DateTime Timestamp { get; set; }
        public int Priority { get; set; }
        public uint BidAmount { get; set; }
        public bool? Successful { get; set; }

        public PickupBid ToDomain(Publisher publisher, MasterGame masterGame, LeagueYear leagueYear)
        {
            Instant instant = LocalDateTime.FromDateTime(Timestamp).InZoneStrictly(DateTimeZone.Utc).ToInstant();
            return new PickupBid(BidID, publisher, leagueYear, masterGame, BidAmount, Priority, instant, Successful);
        }
    }
}
