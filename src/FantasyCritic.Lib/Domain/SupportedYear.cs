namespace FantasyCritic.Lib.Domain
{
    public class SupportedYear : IComparable<SupportedYear>, IEquatable<SupportedYear>
    {
        public SupportedYear(int year, bool openForCreation, bool openForPlay, bool openForBetaUsers, LocalDate startDate, bool finished)
        {
            Year = year;
            OpenForCreation = openForCreation;
            OpenForPlay = openForPlay;
            OpenForBetaUsers = openForBetaUsers;
            StartDate = startDate;
            Finished = finished;
        }

        public int Year { get; }
        public bool OpenForCreation { get; }
        public bool OpenForPlay { get; }
        public bool OpenForBetaUsers { get; }
        public LocalDate StartDate { get; }
        public bool Finished { get; }

        public static bool Year2022FeatureSupported(int year) => year >= 2022;

        public override string ToString()
        {
            return Year.ToString();
        }

        public int CompareTo(SupportedYear other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Year.CompareTo(other.Year);
        }

        public bool Equals(SupportedYear other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Year == other.Year;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SupportedYear)obj);
        }

        public override int GetHashCode()
        {
            return Year;
        }
    }
}
