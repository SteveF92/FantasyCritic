using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Royale;

public class RoyaleYearQuarter : IEquatable<RoyaleYearQuarter>, IComparable<RoyaleYearQuarter>
{
    public RoyaleYearQuarter(YearQuarter yearQuarter, bool openForPlay, bool finished, VeryMinimalFantasyCriticUser? winningUser)
    {
        YearQuarter = yearQuarter;
        OpenForPlay = openForPlay;
        Finished = finished;
        WinningUser = winningUser;
    }

    public YearQuarter YearQuarter { get; }
    public bool OpenForPlay { get; }
    public bool Finished { get; }
    public VeryMinimalFantasyCriticUser? WinningUser { get; }

    public List<string> GetBannedTags()
    {
        List<string> bannedTags = [
            "CurrentlyInEarlyAccess",
            "DirectorsCut",
            "Remaster",
            "Port",
            "ReleasedInternationally",
            "YearlyInstallment"
        ];

        if (SupportedYear.Year2026FeatureSupported(YearQuarter.Year))
        {
            bannedTags.AddRange(
            [
                "PlannedForEarlyAccess",
                "Remake",
                "PartialRemake",
            ]);
        }

        return bannedTags;
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
