using System;
using CSharpFunctionalExtensions;
using NodaTime;

namespace FantasyCritic.Lib.Domain.LeagueActions
{
    public class PickupBid : IEquatable<PickupBid>
    {
        public PickupBid(Guid bidID, Publisher publisher, LeagueYear leagueYear, MasterGame masterGame, Maybe<PublisherGame> conditionalDropPublisherGame,
            uint bidAmount, int priority, Instant timestamp, bool? successful)
        {
            BidID = bidID;
            Publisher = publisher;
            LeagueYear = leagueYear;
            MasterGame = masterGame;
            ConditionalDropPublisherGame = conditionalDropPublisherGame;
            BidAmount = bidAmount;
            Priority = priority;
            Timestamp = timestamp;
            Successful = successful;
        }

        public Guid BidID { get; }
        public Publisher Publisher { get; }
        public LeagueYear LeagueYear { get; }
        public MasterGame MasterGame { get; }
        public Maybe<PublisherGame> ConditionalDropPublisherGame { get; }
        public uint BidAmount { get; }
        public int Priority { get; }
        public Instant Timestamp { get; }
        public bool? Successful { get; }

        public bool ConditionalDropShouldSucceed { get; set; }

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
