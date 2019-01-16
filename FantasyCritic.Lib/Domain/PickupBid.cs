using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class PickupBid : IEquatable<PickupBid>
    {
        public PickupBid(Guid bidID, Publisher publisher, MasterGame masterGame, uint bidAmount, int priority, Instant timestamp, bool? successful)
        {
            BidID = bidID;
            Publisher = publisher;
            MasterGame = masterGame;
            BidAmount = bidAmount;
            Priority = priority;
            Timestamp = timestamp;
            Successful = successful;
        }

        public Guid BidID { get; }
        public Publisher Publisher { get; }
        public MasterGame MasterGame { get; }
        public uint BidAmount { get; }
        public int Priority { get; }
        public Instant Timestamp { get; }
        public bool? Successful { get; }

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
            return Equals((PickupBid) obj);
        }

        public override int GetHashCode()
        {
            return BidID.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Publisher.PublisherName}|{MasterGame.GameName}|{BidAmount}|{Priority}|{Successful}";
        }
    }
}
