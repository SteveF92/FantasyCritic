using System;
using NodaTime;

namespace FantasyCritic.Lib.Domain.LeagueActions
{
    public class DropRequest : IEquatable<DropRequest>
    {
        public DropRequest(Guid dropRequestID, Publisher publisher, LeagueYear leagueYear, MasterGame masterGame, Instant timestamp, bool? successful)
        {
            DropRequestID = dropRequestID;
            Publisher = publisher;
            LeagueYear = leagueYear;
            MasterGame = masterGame;
            Timestamp = timestamp;
            Successful = successful;
        }

        public Guid DropRequestID { get; }
        public Publisher Publisher { get; }
        public LeagueYear LeagueYear { get; }
        public MasterGame MasterGame { get; }
        public Instant Timestamp { get; }
        public bool? Successful { get; }

        public bool Equals(DropRequest other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return DropRequestID.Equals(other.DropRequestID);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DropRequest) obj);
        }

        public override int GetHashCode()
        {
            return DropRequestID.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Publisher.PublisherName}|{MasterGame.GameName}|{Successful}";
        }
    }
}
