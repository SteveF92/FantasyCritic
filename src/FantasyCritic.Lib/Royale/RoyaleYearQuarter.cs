namespace FantasyCritic.Lib.Royale;

public class RoyaleYearQuarter : IEquatable<RoyaleYearQuarter>, IComparable<RoyaleYearQuarter>
{
    public RoyaleYearQuarter(SupportedYear year, YearQuarter yearQuarter, bool openForPlay, bool finished)
    {
        Year = year;
        YearQuarter = yearQuarter;
        OpenForPlay = openForPlay;
        Finished = finished;
    }

    public SupportedYear Year { get; }
    public YearQuarter YearQuarter { get; }
    public bool OpenForPlay { get; }
    public bool Finished { get; }

    public bool HasReleaseDateLimit
    {
        get
        {
            var compare = YearQuarter.CompareTo(new YearQuarter(2022, 3));
            var laterOrEqual = compare >= 0;
            return laterOrEqual;
        }
    }

    public bool HideUnreleasedGames
    {
        get
        {
            var compare = YearQuarter.CompareTo(new YearQuarter(2022, 4));
            var laterOrEqual = compare >= 0;
            return laterOrEqual;
        }
    }

    public override string ToString()
    {
        return YearQuarter.ToString();
    }

    public bool Equals(RoyaleYearQuarter? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(YearQuarter, other.YearQuarter);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((RoyaleYearQuarter)obj);
    }

    public override int GetHashCode()
    {
        return YearQuarter.GetHashCode();
    }

    public int CompareTo(RoyaleYearQuarter? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return Comparer<YearQuarter>.Default.Compare(YearQuarter, other.YearQuarter);
    }
}
