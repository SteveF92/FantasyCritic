namespace FantasyCritic.Lib.Domain
{
    public class LeagueYearKey : IEquatable<LeagueYearKey>
    {
        public LeagueYearKey(Guid leagueID, int year)
        {
            LeagueID = leagueID;
            Year = year;
        }

        public Guid LeagueID { get; }
        public int Year { get; }

        public bool Equals(LeagueYearKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return LeagueID.Equals(other.LeagueID) && Year == other.Year;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LeagueYearKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (LeagueID.GetHashCode() * 397) ^ Year;
            }
        }

        public override string ToString() => $"{LeagueID}-{Year}";
    }
}
